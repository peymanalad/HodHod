using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
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
public class ReportReferralAppService : HodHodAppServiceBase, IReportReferralAppService
{
    private readonly IRepository<ReportReferral, Guid> _referralRepository;
    private readonly IRepository<Report, Guid> _reportRepository;

    public ReportReferralAppService(IRepository<ReportReferral, Guid> referralRepository, IRepository<Report, Guid> reportRepository)
    {
        _referralRepository = referralRepository;
        _reportRepository = reportRepository;
    }

    public async Task<ReportReferralDto> CreateAsync(CreateReportReferralDto input)
    {
        var user = await GetCurrentUserAsync();
        await EnsureReportAccessAsync(input.ReportId, user);
        await ValidateAssignee(input.ReportId, user, input.ReceiverUserId);

        var entity = ObjectMapper.Map<ReportReferral>(input);
        entity.FromUserId = user.Id;
        await _referralRepository.InsertAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();

        var dto = ObjectMapper.Map<ReportReferralDto>(entity);
        dto.SenderUserId = user.Id;
        dto.SenderUserName = user.UserName;

        var receiver = await UserManager.FindByIdAsync(input.ReceiverUserId.ToString());
        dto.ReceiverUserName = receiver?.UserName;

        return dto;
    }

    public async Task<List<ReportReferralDto>> GetReferrals(Guid reportId)
    {
        var user = await GetCurrentUserAsync();
        await EnsureReportAccessAsync(reportId, user);

        var roles = await UserManager.GetRolesAsync(user);

        var query = _referralRepository.GetAll()
            .Where(r => r.ReportId == reportId);

        if (roles.Contains(StaticRoleNames.Host.CityAdmin))
        {
            query = query.Where(r => r.FromUserId == user.Id || r.ToUserId == user.Id);
        }

        var referrals = await query
            .OrderBy(r => r.CreationTime)
            .ToListAsync();

        if (roles.Contains(StaticRoleNames.Host.CityAdmin) && referrals.Count == 0)
        {
            throw new AbpAuthorizationException("Not authorized to view referrals.");
        }

        var dto = ObjectMapper.Map<List<ReportReferralDto>>(referrals);

        var userIds = referrals.Select(r => r.FromUserId)
            .Concat(referrals.Select(r => r.ToUserId))
            .Distinct()
            .ToList();

        var dict = new Dictionary<long, string>();
        foreach (var id in userIds)
        {
            var u = await UserManager.FindByIdAsync(id.ToString());
            if (u != null)
            {
                dict[id] = u.UserName;
            }
        }

        foreach (var r in dto)
        {
            if (dict.TryGetValue(r.SenderUserId, out var s))
            {
                r.SenderUserName = s;
            }
            if (dict.TryGetValue(r.ReceiverUserId, out var t))
            {
                r.ReceiverUserName = t;
            }
        }

        return dto;
    }

    public async Task<ReportAssignableUsersDto> GetAssignableUsers(Guid reportId)
    {
        var user = await GetCurrentUserAsync();
        await EnsureReportAccessAsync(reportId, user);

        var report = await _reportRepository.FirstOrDefaultAsync(reportId);
        if (report == null)
        {
            throw new EntityNotFoundException("Report not found");
        }

        var roles = await UserManager.GetRolesAsync(user);
        var result = new ReportAssignableUsersDto();

        var superAdmins = await UserManager.GetUsersInRoleAsync(StaticRoleNames.Host.SuperAdmin);
        var provinceAdmins = await UserManager.GetUsersInRoleAsync(StaticRoleNames.Host.ProvinceAdmin);
        var cityAdmins = await UserManager.GetUsersInRoleAsync(StaticRoleNames.Host.CityAdmin);

        if (roles.Contains(StaticRoleNames.Host.SuperAdmin))
        {
            result.Peers = superAdmins.Where(u => u.Id != user.Id)
                .Select(u => new SimpleUserDto { Id = u.Id, UserName = u.UserName }).ToList();

            result.Subordinates.AddRange(
                provinceAdmins.Where(u => u.Province == report.Province)
                    .Select(u => new SimpleUserDto { Id = u.Id, UserName = u.UserName }));

            result.Subordinates.AddRange(
                cityAdmins.Where(u => u.Province == report.Province)
                    .Select(u => new SimpleUserDto { Id = u.Id, UserName = u.UserName }));
        }
        else if (roles.Contains(StaticRoleNames.Host.ProvinceAdmin))
        {
            result.Superiors = superAdmins
                .Select(u => new SimpleUserDto { Id = u.Id, UserName = u.UserName }).ToList();

            result.Peers = provinceAdmins.Where(u => u.Id != user.Id)
                .Select(u => new SimpleUserDto { Id = u.Id, UserName = u.UserName }).ToList();

            result.Subordinates = cityAdmins.Where(u => u.Province == user.Province)
                .Select(u => new SimpleUserDto { Id = u.Id, UserName = u.UserName }).ToList();
        }
        else if (roles.Contains(StaticRoleNames.Host.CityAdmin))
        {
            result.Superiors = provinceAdmins.Where(u => u.Province == user.Province)
                .Select(u => new SimpleUserDto { Id = u.Id, UserName = u.UserName }).ToList();

            result.Peers = cityAdmins.Where(u => u.Id != user.Id && u.Province == user.Province)
                .Select(u => new SimpleUserDto { Id = u.Id, UserName = u.UserName }).ToList();
        }

        return result;
    }

    private async Task EnsureReportAccessAsync(Guid reportId, Authorization.Users.User user)
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

    private async Task ValidateAssignee(Guid reportId, User currentUser, long assigneeId)
    {
        var assignee = await UserManager.FindByIdAsync(assigneeId.ToString());
        if (assignee == null)
        {
            throw new EntityNotFoundException("Assigned user not found");
        }

        await EnsureReportAccessAsync(reportId, assignee);

        var roles = await UserManager.GetRolesAsync(currentUser);
        var assigneeRoles = await UserManager.GetRolesAsync(assignee);

        if (roles.Contains(StaticRoleNames.Host.SuperAdmin))
        {
            return;
        }

        if (roles.Contains(StaticRoleNames.Host.ProvinceAdmin))
        {
            if (assigneeRoles.Contains(StaticRoleNames.Host.SuperAdmin))
            {
                return;
            }

            if (assigneeRoles.Contains(StaticRoleNames.Host.ProvinceAdmin))
            {
                return;
            }

            if (assigneeRoles.Contains(StaticRoleNames.Host.CityAdmin) && assignee.Province == currentUser.Province)
            {
                return;
            }
        }

        if (roles.Contains(StaticRoleNames.Host.CityAdmin))
        {
            if (assigneeRoles.Contains(StaticRoleNames.Host.ProvinceAdmin) && assignee.Province == currentUser.Province)
            {
                return;
            }

            if (assigneeRoles.Contains(StaticRoleNames.Host.CityAdmin) && assignee.Province == currentUser.Province)
            {
                return;
            }
        }

        throw new AbpAuthorizationException("Not authorized to assign to this user");
    }
}