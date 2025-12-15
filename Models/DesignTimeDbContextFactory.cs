using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NEXUSDataLayerScaffold.Models
{
    /// <summary>
    /// Provides a design-time factory for EF Core tools so that migrations can be created
    /// without requiring the full application host (and unrelated services like MinIO).
    /// </summary>
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<NexusLarpLocalContext>
    {
        public NexusLarpLocalContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<NexusLarpLocalContext>();

            // Prefer environment-provided connection string; fall back to a local placeholder.
            var conn = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
                       ?? Environment.GetEnvironmentVariable("POSTGRES_CONNECTION")
                       ?? "Host=localhost;Port=5432;Database=nexus_local;Username=nexus;Password=nexus";

            optionsBuilder.UseNpgsql(conn);
            return new NexusLarpLocalContext(optionsBuilder.Options);
        }
    }
}
