using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using HodHod.Authorization.Roles;
using HodHod.Authorization;
using HodHod.Reports.Dto;
using Microsoft.EntityFrameworkCore;

namespace HodHod.Reports;

[AbpAuthorize]
public class ReportNoteAppService : HodHodAppServiceBase, IReportNoteAppService
{
    private readonly IRepository<ReportNote, Guid> _noteRepository;
    private readonly IRepository<Report, Guid> _reportRepository;

    public ReportNoteAppService(IRepository<ReportNote, Guid> noteRepository, IRepository<Report, Guid> reportRepository)
    {
        _noteRepository = noteRepository;
        _reportRepository = reportRepository;
    }

    public async Task<List<ReportNoteDto>> GetNotes(Guid reportId)
    {
        var user = await GetCurrentUserAsync();
        await EnsureReportAccessAsync(reportId, user);

        var notes = await _noteRepository.GetAll()
            .Where(n => n.ReportId == reportId)
            .OrderBy(n => n.CreationTime)
            .ToListAsync();

        var dto = ObjectMapper.Map<List<ReportNoteDto>>(notes);

        var userIds = notes
            .Where(n => n.CreatorUserId.HasValue)
            .Select(n => n.CreatorUserId.Value)
            .Distinct()
            .ToList();

        var userNameDict = new Dictionary<long, string>();
        var userRoleDict = new Dictionary<long, string>();
        foreach (var id in userIds)
        {
            var u = await UserManager.FindByIdAsync(id.ToString());
            if (u != null)
            {
                userNameDict[id] = u.UserName;
                var roles = await UserManager.GetRolesAsync(u);
                userRoleDict[id] = roles.FirstOrDefault();
            }
        }

        foreach (var n in dto)
        {
            if (n.CreatorUserId.HasValue)
            {
                if (userNameDict.TryGetValue(n.CreatorUserId.Value, out var name))
                {
                    n.CreatorFullName = name;
                }
                if (userRoleDict.TryGetValue(n.CreatorUserId.Value, out var role))
                {
                    n.CreatorRoleName = role;
                }
            }
        }

        return dto;
    }

    public async Task<ReportNoteDto> AddNote(CreateReportNoteDto input)
    {
        var user = await GetCurrentUserAsync();
        await EnsureReportAccessAsync(input.ReportId, user);

        var entity = ObjectMapper.Map<ReportNote>(input);
        entity.CreatorUserId = user.Id;
        await _noteRepository.InsertAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();

        var dto = ObjectMapper.Map<ReportNoteDto>(entity);
        dto.CreatorUserId = user.Id;
        dto.CreatorFullName = $"{user.Name} {user.Surname}";
        dto.CreatorRoleName = (await UserManager.GetRolesAsync(user)).FirstOrDefault();
        dto.CreationTime = entity.CreationTime;
        return dto;
    }

    public async Task<ReportNoteDto> UpdateNote(UpdateReportNoteDto input)
    {
        var user = await GetCurrentUserAsync();
        var entity = await _noteRepository.FirstOrDefaultAsync(input.Id);
        if (entity == null)
        {
            throw new EntityNotFoundException("ReportNote not found");
        }

        await EnsureReportAccessAsync(entity.ReportId, user);
        if (entity.CreatorUserId != user.Id)
        {
            throw new AbpAuthorizationException("Only the creator can edit this note");
        }

        entity.Text = input.Text;
        await _noteRepository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();

        var dto = ObjectMapper.Map<ReportNoteDto>(entity);
        dto.CreatorUserId = entity.CreatorUserId;
        dto.CreatorFullName = $"{user.Name} {user.Surname}";
        dto.CreatorRoleName = (await UserManager.GetRolesAsync(user)).FirstOrDefault();
        return dto;
    }

    public async Task DeleteNote(EntityDto<Guid> input)
    {
        var user = await GetCurrentUserAsync();
        var entity = await _noteRepository.FirstOrDefaultAsync(input.Id);
        if (entity == null)
        {
            throw new EntityNotFoundException("ReportNote not found");
        }

        await EnsureReportAccessAsync(entity.ReportId, user);
        if (entity.CreatorUserId != user.Id)
        {
            throw new AbpAuthorizationException("Only the creator can delete this note");
        }

        await _noteRepository.DeleteAsync(entity);
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
}