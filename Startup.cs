using System;
using System.Collections.Generic;
using System.Net;
using Grafana.OpenTelemetry;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
        services.AddOpenTelemetry().WithTracing(configure =>
            {
                configure.UseGrafana();
            })
            .WithMetrics(configure =>
            {
                configure.UseGrafana();
            });

        services.AddLogging(configure =>
            configure.AddOpenTelemetry(options =>
                {
                    options.UseGrafana();
                })
            );


        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        var tok = new BearerToken();

        var domain = $"https://{_config["Auth0:Domain"]}/";
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.Authority = domain;
            options.Audience = _config["Auth0:ApiIdentifier"];
            options.SaveToken = true;

            options.TokenValidationParameters = new TokenValidationParameters
            {
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


        //services.AddSwaggerGen(c =>
        //{
        //    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Nexus API V1", Version = "v1" });
        //});


        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Nexus API V1", Version = "v1" });
            c.OperationFilter<OpenApiParameterIgnoreFilter>();
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      \r\n\r\nExample: 'Bearer 12345abcdef'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
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
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header
                    },
                    new List<string>()
                }
            });
            //var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            //var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            //c.IncludeXmlComments(xmlPath);
        });


        // Add authentication services
        //            services.AddAuthentication(options => {
        //                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        //                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        //            })
        //            .AddCookie()
        //            .AddOpenIdConnect("Auth0", options =>
        //            {
        // Set the authority to your Auth0 domain
        //                options.Authority = $"https://{Configuration["Auth0:Domain"]}";

        // Configure the Auth0 Client ID and Client Secret
        //                options.ClientId = Configuration["Auth0:ClientId"];
        //                options.ClientSecret = Configuration["Auth0:ClientSecret"];

        // Set response type to code
        //                options.ResponseType = "code";

        // Configure the scope
        //                options.Scope.Clear();
        //                options.Scope.Add("openid");
        //                options.Scope.Add("email");

        // Set the callback path, so Auth0 will call back to http://localhost:5555/callback
        // Also ensure that you have added the URL as an Allowed Callback URL in your Auth0 dashboard
        //options.CallbackPath = new PathString("/callback");

        // Configure the Claims Issuer to be Auth0
        //                options.ClaimsIssuer = "Auth0";

        // Saves tokens to the AuthenticationProperties
        //                options.SaveTokens = true;

        //            });

        services.AddMvc();
        services.AddRazorPages();


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

        
        
        //connstring = "Host=localhost;Port=5432;Database=NexusLARP_Local;Username=postgres;Password=L4RPEverywhere!";
        accesskey = "S6epybsl7DRwSNmstqaq";
        secretkey = "UCMzYZCJ1pXG5AQ9eAEsDmfAYkeeCPr4vWjba9EM";
        // LOCAL DOCKER CONNSTTRING
        //connstring = "Host=LARPpi;Port=32775;Database=NexusLARP;Username=postgres;Password=L4RPEverywhere!";

        //if (host != _config.GetValue<string>("ConnectionSrings:Host"))
        //    connstring += ";SslMode=allow;Trust Server Certificate=true;";

        // string connstring = _config.GetValue<string>("ConnectionSrings:NexusDBConnectionString");
        services.AddDbContext<NexusLarpLocalContext>(options => options.UseNpgsql(connstring)
        );
        services.AddMvc(x => x.EnableEndpointRouting = false);
        services.AddMinio(configure =>
        {
        configure.WithEndpoint("decade.kylebrighton.com:9000")
                 .WithCredentials(accesskey, secretkey);
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
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
                .WithOrigins("https://decade.kylebrighton.com:3000", "http://localhost:3000")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
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

        //app.UseHttpsRedirection();
        //app.UseStaticFiles();
        app.UseRouting();
        //app.UseCertificateForwarding();
        //app.UseCookiePolicy();
        app.UseAuthentication();
        app.UseAuthorization();
        // app.UseEndpoints(endpoints =>
        // {
        //     endpoints.MapControllers();

        // });
        app.UseMvc(routes =>
        {
            routes.MapRoute(
                "default",
                "{controller=Home}/{action=Index}/{id?}");
        });

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }
}