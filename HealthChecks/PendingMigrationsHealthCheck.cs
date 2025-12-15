using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NEXUSDataLayerScaffold.Models;

namespace NEXUSDataLayerScaffold.HealthChecks
{
    public sealed class PendingMigrationsHealthCheck : IHealthCheck
    {
        private readonly NexusLarpLocalContext _db;

        public PendingMigrationsHealthCheck(NexusLarpLocalContext db)
        {
            _db = db;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var pending = _db.Database.GetPendingMigrations();
            if (pending != null && pending.Any())
            {
                var detail = $"Pending EF migrations: {string.Join(", ", pending)}";
                return Task.FromResult(HealthCheckResult.Degraded(detail));
            }
            return Task.FromResult(HealthCheckResult.Healthy("No pending EF migrations"));
        }
    }
}
