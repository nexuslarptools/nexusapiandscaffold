using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

namespace NEXUSDataLayerScaffold;

public class Program
{
    //static HttpClient client = new HttpClient();


    public static void Main(string[] args)
    {
        // Ensure W3C Trace Context is used and recognized
        Activity.DefaultIdFormat = ActivityIdFormat.W3C;
        Activity.ForceDefaultIdFormat = true;

        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder =>
        {
            //-- never undo this
            //webBuilder.UseStartup<Startup>().UseUrls("https://192.168.254.11:6001", "http://192.168.254.11:6002");
            webBuilder.UseStartup<Startup>().UseUrls("http://*:80;http://*:443");
        });
    }
}