using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Authorization;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using HodHod.Authorization;
using HodHod.Authorization.Roles;
using HodHod.Authorization.Users;
using HodHod.Notifications;
using HodHod.Reports.Dto;
using Microsoft.EntityFrameworkCore;

namespace HodHod.Reports;

[AbpAuthorize]
public class ReportReferralAppService : HodHodAppServiceBase, IReportReferralAppService
{
    private readonly IRepository<ReportReferral, Guid> _referralRepository;
    private readonly IRepository<Report, Guid> _reportRepository;
    private readonly IAppNotifier _appNotifier;
    private readonly IReportHistoryManager _historyManager;

    public ReportReferralAppService(
        IRepository<ReportReferral, Guid> referralRepository,
        IRepository<Report, Guid> reportRepository,
        IAppNotifier appNotifier,
        IReportHistoryManager historyManager)
    {
        _referralRepository = referralRepository;
        _reportRepository = reportRepository;
        _appNotifier = appNotifier;
        _historyManager = historyManager;
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
        dto.SenderFullName = $"{user.Name} {user.Surname}";

        var receiver = await UserManager.FindByIdAsync(input.ReceiverUserId.ToString());
        dto.ReceiverFullName = receiver != null ? $"{receiver.Name} {receiver.Surname}" : null;

        if (receiver != null)
        {
            await _appNotifier.SendMessageAsync(
                AppNotificationNames.ReportReferral,
                $"گزارش {input.UniqueId} به شما ارجاع داده شد.",
                new[] { receiver.ToUserIdentifier() });
        }

        await _historyManager.LogAsync(input.ReportId, user.Id, $"{user.Name} {user.Surname}", ReportActionType.ReportReferral,
            $"ارجاع شد به {dto.ReceiverFullName ?? input.ReceiverUserId.ToString()}", ReportHistoryVisibility.All);


        return dto;
    }

    public async Task<ReportReferralsDto> GetReferrals(Guid reportId)
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
                dict[id] = $"{u.Name} {u.Surname}";
            }
        }

        foreach (var r in dto)
        {
            if (dict.TryGetValue(r.SenderUserId, out var s))
            {
                r.SenderFullName = s;
            }
            if (dict.TryGetValue(r.ReceiverUserId, out var t))
            {
                r.ReceiverFullName = t;
            }
        }

        var result = new ReportReferralsDto
        {
            All = dto,
            Inbox = dto.Where(r => r.ReceiverUserId == user.Id).ToList(),
            Outbox = dto.Where(r => r.SenderUserId == user.Id).ToList()
        };

        return result;
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
                .Select(u => new SimpleUserDto { Id = u.Id, FullName = $"{u.Name} {u.Surname}" }).ToList();

            result.Subordinates.AddRange(
                provinceAdmins.Where(u => u.Province == report.Province)
                    .Select(u => new SimpleUserDto { Id = u.Id, FullName = $"{u.Name} {u.Surname}" }));

            result.Subordinates.AddRange(
                cityAdmins.Where(u => u.Province == report.Province)
                    .Select(u => new SimpleUserDto { Id = u.Id, FullName = $"{u.Name} {u.Surname}" }));
        }
        else if (roles.Contains(StaticRoleNames.Host.ProvinceAdmin))
        {
            result.Superiors = superAdmins
                .Select(u => new SimpleUserDto { Id = u.Id, FullName = $"{u.Name} {u.Surname}" }).ToList();

            result.Peers = provinceAdmins.Where(u => u.Id != user.Id)
                .Select(u => new SimpleUserDto { Id = u.Id, FullName = $"{u.Name} {u.Surname}" }).ToList();

            result.Subordinates = cityAdmins.Where(u => u.Province == user.Province)
                .Select(u => new SimpleUserDto { Id = u.Id, FullName = $"{u.Name} {u.Surname}" }).ToList();
        }
        else if (roles.Contains(StaticRoleNames.Host.CityAdmin))
        {
            result.Superiors = provinceAdmins.Where(u => u.Province == user.Province)
                .Select(u => new SimpleUserDto { Id = u.Id, FullName = $"{u.Name} {u.Surname}" }).ToList();

            result.Peers = cityAdmins.Where(u => u.Id != user.Id && u.Province == user.Province)
                .Select(u => new SimpleUserDto { Id = u.Id, FullName = $"{u.Name} {u.Surname}" }).ToList();
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