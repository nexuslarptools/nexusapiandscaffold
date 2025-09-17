using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

namespace NEXUSDataLayerScaffold;

public class Program
{
    //static HttpClient client = new HttpClient();


    public static void Main(string[] args)
    {
        // Ensure W3C Trace Context is used and recognized
        Activity.DefaultIdFormat = ActivityIdFormat.W3C;
        Activity.ForceDefaultIdFormat = true;

        // Ensure W3C TraceContext and Baggage propagation for incoming/outgoing requests
        Sdk.SetDefaultTextMapPropagator(new CompositeTextMapPropagator(new TextMapPropagator[]
        {
            new TraceContextPropagator(),
            new BaggagePropagator()
        }));

        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                // Map standard AUTH0_* environment variables into the hierarchical Auth0:* configuration keys
                var map = new Dictionary<string, string?>();

                void Map(string key, string env)
                {
                    var value = Environment.GetEnvironmentVariable(env);
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        map[$"Auth0:{key}"] = value;
                    }
                }

                Map("Domain", "AUTH0_DOMAIN");
                Map("Audience", "AUTH0_AUDIENCE");
                Map("ApiIdentifier", "AUTH0_API_IDENTIFIER");
                Map("ClientId", "AUTH0_CLIENT_ID");
                Map("Secret", "AUTH0_CLIENT_SECRET");
                Map("UserInfoEndpoint", "AUTH0_USERINFO_ENDPOINT");

                if (map.Count > 0)
                {
                    // Values from this provider will override appsettings.json placeholders
                    config.AddInMemoryCollection(map);
                }
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                //-- never undo this
                //webBuilder.UseStartup<Startup>().UseUrls("https://192.168.254.11:6001", "http://192.168.254.11:6002");
                webBuilder.UseStartup<Startup>().UseUrls("http://*:80;http://*:443");
            });
    }
}