using System;
using System.Threading.RateLimiting;
using HodHod.Reports;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace HodHod.Web.Reports;

public static class ReportDependencyInjectionExtensions
{
    public static void AddReportRateLimit(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy("ReportLimiter", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString(),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = ReportRateLimitConsts.PermitLimit,
                        Window = TimeSpan.FromSeconds(ReportRateLimitConsts.WindowSeconds)
                    }
                )
            );
        });
    }
}