using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NEXUSDataLayerScaffold.Models;

namespace NEXUSDataLayerScaffold.Tests.Infrastructure;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Override authentication defaults to use the test handler
            services.PostConfigureAll<AuthenticationOptions>(o =>
            {
                o.DefaultAuthenticateScheme = TestAuthHandler.DefaultScheme;
                o.DefaultChallengeScheme = TestAuthHandler.DefaultScheme;
                o.DefaultScheme = TestAuthHandler.DefaultScheme;
                o.DefaultSignInScheme = TestAuthHandler.DefaultScheme;
            });

            services.AddAuthentication(TestAuthHandler.DefaultScheme)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.DefaultScheme, _ => { });

            // Swap DbContext to in-memory
            var toRemove = services.Where(d => d.ServiceType == typeof(DbContextOptions<NexusLarpLocalContext>)).ToList();
            foreach (var d in toRemove) services.Remove(d);

            services.AddDbContext<NexusLarpLocalContext>(opts =>
            {
                opts.UseInMemoryDatabase("NexusTests");
            });

            // Build provider and seed database
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<NexusLarpLocalContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
            TestDataSeeder.Seed(db);
        });
    }
}
