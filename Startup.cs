using System;
using System.Collections.Generic;
using System.Net;
using Grafana.OpenTelemetry;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Minio;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NEXUSDataLayerScaffold.Attributes;
using NEXUSDataLayerScaffold.Entities;
using NEXUSDataLayerScaffold.Models;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;

namespace NEXUSDataLayerScaffold;

public class Startup
{
    private readonly IConfiguration _config;

    public Startup(IConfiguration configuration)
    {
        _config = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
        //services.AddTransient<SampleMiddleware>();
        //services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

        services.AddControllers().AddNewtonsoftJson(options =>
        {
            // Use the default property (Pascal) casing
            options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        });
        // Health checks for liveness and readiness
        services.AddHealthChecks();
        services.AddOpenTelemetry().ConfigureResource(rb =>
            {
                var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
                var version = typeof(Startup).Assembly.GetName().Version?.ToString() ?? "0.0.0";
                rb.AddService(serviceName: "NEXUSDataLayerScaffold", serviceVersion: version);
                rb.AddAttributes(new []
                {
                    new KeyValuePair<string, object>("deployment.environment", env),
                    new KeyValuePair<string, object>("service.instance.id", Environment.MachineName),
                    new KeyValuePair<string, object>("service.namespace", "NEXUS")
                });
            })
            .WithTracing(configure =>
            {
                // Ensure incoming traceparent is honored and outgoing contexts are propagated
                configure.AddAspNetCoreInstrumentation(options =>
                {
                    options.RecordException = true;
                });
                configure.AddHttpClientInstrumentation();
                configure.UseGrafana();
            })
            .WithMetrics(configure =>
            {
                configure.UseGrafana();
            });

        services.AddLogging(configure =>
            {
                // Keep OpenTelemetry logging exporter (Grafana / OTLP via Grafana.OpenTelemetry)
                configure.AddOpenTelemetry(options =>
                {
                    options.IncludeFormattedMessage = true;
                    options.IncludeScopes = true;
                    options.ParseStateValues = true;
                    options.UseGrafana();
                });
                // Enable console logging for local troubleshooting (Development only)
                var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
                if (string.Equals(envName, "Development", StringComparison.OrdinalIgnoreCase))
                {
                    configure.AddConsole();
                }
            }
        );

        // Honor reverse-proxy headers (e.g., Traefik) for original client IP and scheme
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

            // Allow specifying known proxy IPs via configuration/env: ReverseProxy:KnownProxies=["10.0.0.10","192.168.1.2"]
            var knownProxyStrings = _config.GetSection("ReverseProxy:KnownProxies").Get<string[]>() ?? Array.Empty<string>();
            foreach (var proxy in knownProxyStrings)
            {
                if (IPAddress.TryParse(proxy, out var ip))
                {
                    options.KnownProxies.Add(ip);
                }
            }

            // In Development, if no proxies provided, allow all forwarded headers (do NOT use this in Production)
            var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            if (knownProxyStrings.Length == 0 && string.Equals(envName, "Development", StringComparison.OrdinalIgnoreCase))
            {
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            }
        });

        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        var tok = new BearerToken();

        // Bind and validate Auth0 options; fail fast on missing values
        services.AddOptions<Auth0Options>()
            .Bind(_config.GetSection("Auth0"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        var auth0 = _config.GetSection("Auth0").Get<Auth0Options>();

        var domain = $"https://{auth0.Domain}/";
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.Authority = domain;
            options.Audience = auth0.ApiIdentifier;
            options.SaveToken = false;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                NameClaimType = "name",
                RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/roles"
            };
            //options.Events = new JwtBearerEvents
            // {
            //     OnMessageReceived = context =>
            //     {
            //         var accessToken = context.Request.Query["access_token"];

            //         tok.Token = accessToken.ToString();


            //         return Task.CompletedTask;

            //     }
            // };
        });

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.SameSite = SameSiteMode.None;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });




        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Nexus API V1", Version = "v1" });
            c.OperationFilter<OpenApiParameterIgnoreFilter>();
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
            // Enable XML comments if the documentation file is generated in the project
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (System.IO.File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }
        });




        // HTTP request logging
        services.AddHttpLogging(options =>
        {
            options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestPropertiesAndHeaders |
                                     Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponsePropertiesAndHeaders;
            options.MediaTypeOptions.AddText("application/json");
            options.RequestHeaders.Add("traceparent");
            options.ResponseHeaders.Add("traceparent");
        });


        //var builder = new NpgsqlConnectionStringBuilder(_config.GetValue<string>("ConnectionSrings:NexusDBConnectionString"));

        //var builder = new NpgsqlConnectionStringBuilder();
        //builder.Host = "nexusapiandscaffold_db_1...";


        //{
        //    Password = dbPassword
        //};
        //services.AddControllers()
        //    .AddNewtonsoftJson();
        //services.AddDbContext<NexusLARPContextBase>(options => options.UseNpgsql(builder.ConnectionString));

        var host = Environment.GetEnvironmentVariable("Host") ??
                   _config.GetValue<string>("ConnectionSrings:Host");
        var port = Environment.GetEnvironmentVariable("Port") ??
                   _config.GetValue<string>("ConnectionSrings:Port");
        var database = Environment.GetEnvironmentVariable("Database") ??
                       _config.GetValue<string>("ConnectionSrings:Database");
        var username = Environment.GetEnvironmentVariable("DBUsername") ??
                       _config.GetValue<string>("ConnectionSrings:DBUsername");
        var password = Environment.GetEnvironmentVariable("Password") ??
                       _config.GetValue<string>("ConnectionSrings:Password");
        var accesskey = Environment.GetEnvironmentVariable("accesskey") ??
               _config.GetValue<string>("ConnectionSrings:accesskey");
        var secretkey = Environment.GetEnvironmentVariable("secretkey") ??
               _config.GetValue<string>("ConnectionSrings:secretkey");

        var connstring = "Host=" + host + ";Port=" + port
                         + ";Database=" + database + "; Username=" + username + ";Password=" + password;

        
        
        //connstring = "Host=localhost;Port=5433;Database=NexusLarp_Local_FromNick2;Username=postgres;Password=******";
        // NOTE: Use environment variables or configuration to provide MinIO access/secret keys. Removed hardcoded overrides for security.
        // LOCAL DOCKER CONNSTTRING
        //connstring = "Host=LARPpi;Port=32775;Database=NexusLARP;Username=postgres;Password=******";

        //if (host != _config.GetValue<string>("ConnectionSrings:Host"))
        //    connstring += ";SslMode=allow;Trust Server Certificate=true;";

        // string connstring = _config.GetValue<string>("ConnectionSrings:NexusDBConnectionString");
        services.AddDbContext<NexusLarpLocalContext>(options => options.UseNpgsql(connstring)
        );
        services.AddMinio(configure =>
        {
        configure.WithEndpoint("decade.kylebrighton.com:9000")
                 .WithCredentials(accesskey, secretkey);
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
    {
        // Process X-Forwarded-* headers from Traefik BEFORE anything else (important for HTTPS redirection and auth)
        app.UseForwardedHeaders();

        if (env.IsDevelopment())
            app.UseDeveloperExceptionPage();
        else
            app.UseHsts();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseCors(policy => policy
                .AllowAnyHeader()
                .AllowAnyMethod()
                //.SetIsOriginAllowed(origin => true) // allow any origin
                //.WithOrigins("http://localhost:3000", "http://localhost:8080", "http://10.0.0.175:8080", "http://10.0.0.175:3000")
                //.AllowCredentials()
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .WithExposedHeaders("traceparent", "tracestate", "Server-Timing")
            );

            //app.UseCors(builder => builder
            //   .AllowAnyOrigin()
            //   .AllowAnyMethod()
            //   .AllowAnyHeader()
            //   .AllowCredentials());
        }
        else
        {
            app.UseCors(builder => builder
                //.SetIsOriginAllowed(origin => true) // allow any origin
                .WithOrigins("https://decade.kylebrighton.com:3000", "http://localhost:3000","https://databasebackenddev.kylebrighton.com","https://databasedev.kylebrighton.com")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
                .WithExposedHeaders("traceparent", "tracestate", "Server-Timing"));
        }

        // Enable middleware to serve generated Swagger as a JSON endpoint.
        app.UseSwagger();


        // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
        // specifying the Swagger JSON endpoint.
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Nexus API V1");
            c.RoutePrefix = string.Empty;
        });

        // Global exception handling to log and return ProblemDetails
        app.Use(async (context, next) =>
        {
            try
            {
                await next();
            }
            catch (Exception ex)
            {
                var activity = System.Diagnostics.Activity.Current;
                var traceId = activity?.TraceId.ToString() ?? context.TraceIdentifier;
                logger.LogError(ex, "Unhandled exception for {Path} TraceId={TraceId}", context.Request.Path, traceId);

                if (!context.Response.HasStarted)
                {
                    context.Response.Clear();
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/problem+json";
                    var problem = new
                    {
                        type = "https://httpstatuses.com/500",
                        title = "An unexpected error occurred.",
                        status = 500,
                        traceId
                    };
                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(problem);
                    await context.Response.WriteAsync(json);
                }
            }
        });

        app.UseHttpsRedirection();
        //app.UseStaticFiles();
        app.UseRouting();
        // Emit structured request logs
        app.UseHttpLogging();
        //app.UseCertificateForwarding();
        //app.UseCookiePolicy();
        // Add a simple logging scope that carries correlation info and expose trace and server-timing headers for frontend
        app.Use(async (context, next) =>
        {
            var activity = System.Diagnostics.Activity.Current;
            var traceId = activity?.TraceId.ToString() ?? context.TraceIdentifier;
            var sw = System.Diagnostics.Stopwatch.StartNew();

            // Ensure response includes current trace context and server-timing so browsers can correlate
            context.Response.OnStarting(() =>
            {
                sw.Stop();
                if (activity != null)
                {
                    // Per W3C spec, traceparent is the Activity Id in W3C format
                    context.Response.Headers["traceparent"] = activity.Id;
                    if (!string.IsNullOrEmpty(activity.TraceStateString))
                    {
                        context.Response.Headers["tracestate"] = activity.TraceStateString;
                    }
                    // Add trace context to Server-Timing as descriptor (helps browser devtools link traces)
                    context.Response.Headers.Append("Server-Timing", $"traceparent;desc=\"{activity.Id}\"");
                }
                // Always add app processing duration in milliseconds
                context.Response.Headers.Append("Server-Timing", $"app;dur={sw.Elapsed.TotalMilliseconds:0.###}");
                return System.Threading.Tasks.Task.CompletedTask;
            });

            using (logger.BeginScope(new Dictionary<string, object>
            {
                ["TraceId"] = traceId,
                ["RequestPath"] = context.Request.Path.ToString(),
            }))
            {
                await next();
            }
        });
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHealthChecks("/health");
            endpoints.MapHealthChecks("/health/ready");
        });

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }
}