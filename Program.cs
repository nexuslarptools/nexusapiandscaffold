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
            webBuilder.UseStartup<Startup>().UseUrls("https://192.168.254.11:6001", "http://192.168.254.11:6002");
            //webBuilder.UseStartup<Startup>().UseUrls("http://*:80;http://*:443");
        });
    }
}