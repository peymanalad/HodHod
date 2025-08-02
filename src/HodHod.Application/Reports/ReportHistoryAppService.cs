using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using HodHod.Authorization;
using HodHod.Authorization.Roles;
using HodHod.Authorization.Users;
using HodHod.Reports.Dto;
using Microsoft.EntityFrameworkCore;

namespace HodHod.Reports;

[AbpAuthorize]
public class ReportHistoryAppService : HodHodAppServiceBase, IReportHistoryAppService
{
    private readonly IRepository<ReportHistoryLog, Guid> _logRepository;
    private readonly IRepository<Report, Guid> _reportRepository;

    public ReportHistoryAppService(IRepository<ReportHistoryLog, Guid> logRepository, IRepository<Report, Guid> reportRepository)
    {
        _logRepository = logRepository;
        _reportRepository = reportRepository;
    }

    public async Task<List<ReportHistoryLogDto>> GetHistory(Guid reportId)
    {
        var user = await GetCurrentUserAsync();
        await EnsureReportAccessAsync(reportId, user);

        var roles = await UserManager.GetRolesAsync(user);
        var query = _logRepository.GetAll().Where(l => l.ReportId == reportId);

        if (roles.Contains(StaticRoleNames.Host.SuperAdmin))
        {
            // no additional filter
        }
        else if (roles.Contains(StaticRoleNames.Host.ProvinceAdmin))
        {
            query = query.Where(l =>
                l.Visibility.HasFlag(ReportHistoryVisibility.ProvinceAdmin) ||
                (l.Visibility.HasFlag(ReportHistoryVisibility.PerformedUser) && l.PerformedByUserId == user.Id));
        }
        else if (roles.Contains(StaticRoleNames.Host.CityAdmin))
        {
            query = query.Where(l =>
                l.Visibility.HasFlag(ReportHistoryVisibility.CityAdmin) ||
                (l.Visibility.HasFlag(ReportHistoryVisibility.PerformedUser) && l.PerformedByUserId == user.Id));
        }

        var list = await query.OrderByDescending(l => l.ActionTime).ToListAsync();
        return ObjectMapper.Map<List<ReportHistoryLogDto>>(list);
    }

    private async Task EnsureReportAccessAsync(Guid reportId, User user)
    {
        var roles = await UserManager.GetRolesAsync(user);
        if (!(roles.Contains(StaticRoleNames.Host.Admin) ||
              roles.Contains(StaticRoleNames.Host.SuperAdmin) ||
              roles.Contains(StaticRoleNames.Host.ProvinceAdmin) ||
              roles.Contains(StaticRoleNames.Host.CityAdmin)))
        {
            throw new AbpAuthorizationException("Not authorized to view reports.");
        }

        if (roles.Contains(StaticRoleNames.Host.CityAdmin))
        {
            var report = await _reportRepository.FirstOrDefaultAsync(reportId);
            if (report != null && !string.IsNullOrEmpty(user.City) && report.City != user.City)
            {
                throw new AbpAuthorizationException("Not authorized to access this report");
            }
        }
        else if (roles.Contains(StaticRoleNames.Host.ProvinceAdmin))
        {
            var report = await _reportRepository.FirstOrDefaultAsync(reportId);
            if (report != null && !string.IsNullOrEmpty(user.Province) && report.Province != user.Province)
            {
                throw new AbpAuthorizationException("Not authorized to access this report");
            }
        }
    }
}