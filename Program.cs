using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace NEXUSDataLayerScaffold;

public class Program
{
    //static HttpClient client = new HttpClient();


    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>().UseUrls("https://localhost:6001", "http://localhost:6002");
            //webBuilder.UseStartup<Startup>().UseUrls("http://*:80;http://*:443");
        });
    }
}