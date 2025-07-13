using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HodHod.EntityFrameworkCore;

namespace HodHod.HealthChecks;

public class HodHodDbContextHealthCheck : IHealthCheck
{
    private readonly DatabaseCheckHelper _checkHelper;

    public HodHodDbContextHealthCheck(DatabaseCheckHelper checkHelper)
    {
        _checkHelper = checkHelper;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        if (_checkHelper.Exist("db"))
        {
            return Task.FromResult(HealthCheckResult.Healthy("HodHodDbContext connected to database."));
        }

        return Task.FromResult(HealthCheckResult.Unhealthy("HodHodDbContext could not connect to database"));
    }
}
