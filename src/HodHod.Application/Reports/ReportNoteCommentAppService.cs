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
public class ReportNoteCommentAppService : HodHodAppServiceBase, IReportNoteCommentAppService
{
    private readonly IRepository<ReportNoteComment, Guid> _commentRepository;
    private readonly IRepository<ReportNote, Guid> _noteRepository;
    private readonly IRepository<Report, Guid> _reportRepository;

    public ReportNoteCommentAppService(
        IRepository<ReportNoteComment, Guid> commentRepository,
        IRepository<ReportNote, Guid> noteRepository,
        IRepository<Report, Guid> reportRepository)
    {
        _commentRepository = commentRepository;
        _noteRepository = noteRepository;
        _reportRepository = reportRepository;
    }

    public async Task<List<ReportNoteCommentDto>> GetComments(Guid noteId)
    {
        var user = await GetCurrentUserAsync();
        var note = await _noteRepository.FirstOrDefaultAsync(noteId);
        if (note == null)
        {
            throw new EntityNotFoundException("ReportNote not found");
        }

        await EnsureReportAccessAsync(note.ReportId, user);

        var comments = await _commentRepository.GetAll()
            .Where(c => c.NoteId == noteId)
            .OrderBy(c => c.CreationTime)
            .ToListAsync();

        var dto = ObjectMapper.Map<List<ReportNoteCommentDto>>(comments);

        var userIds = comments
            .Where(c => c.CreatorUserId.HasValue)
            .Select(c => c.CreatorUserId.Value)
            .Distinct()
            .ToList();

        var userNameDict = new Dictionary<long, string>();
        var userRoleDict = new Dictionary<long, string>();
        foreach (var id in userIds)
        {
            var u = await UserManager.FindByIdAsync(id.ToString());
            if (u != null)
            {
                userNameDict[id] = $"{u.Name} {u.Surname}";
                var roles = await UserManager.GetRolesAsync(u);
                userRoleDict[id] = roles.FirstOrDefault();
            }
        }

        foreach (var c in dto)
        {
            if (c.CreatorUserId.HasValue)
            {
                if (userNameDict.TryGetValue(c.CreatorUserId.Value, out var name))
                {
                    c.CreatorFullName = name;
                }
                if (userRoleDict.TryGetValue(c.CreatorUserId.Value, out var role))
                {
                    c.CreatorRoleName = role;
                }
            }
        }

        return dto;
    }

    public async Task<ReportNoteCommentDto> AddComment(CreateReportNoteCommentDto input)
    {
        var user = await GetCurrentUserAsync();
        var note = await _noteRepository.FirstOrDefaultAsync(input.NoteId);
        if (note == null)
        {
            throw new EntityNotFoundException("ReportNote not found");
        }

        await EnsureReportAccessAsync(note.ReportId, user);

        var entity = ObjectMapper.Map<ReportNoteComment>(input);
        entity.CreatorUserId = user.Id;
        await _commentRepository.InsertAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();

        var dto = ObjectMapper.Map<ReportNoteCommentDto>(entity);
        dto.CreatorUserId = user.Id;
        dto.CreatorFullName = $"{user.Name} {user.Surname}";
        dto.CreatorRoleName = (await UserManager.GetRolesAsync(user)).FirstOrDefault();
        dto.CreationTime = entity.CreationTime;
        return dto;
    }

    public async Task<ReportNoteCommentDto> UpdateComment(UpdateReportNoteCommentDto input)
    {
        var user = await GetCurrentUserAsync();
        var entity = await _commentRepository.FirstOrDefaultAsync(input.Id);
        if (entity == null)
        {
            throw new EntityNotFoundException("ReportNoteComment not found");
        }

        var note = await _noteRepository.FirstOrDefaultAsync(entity.NoteId);
        if (note == null)
        {
            throw new EntityNotFoundException("ReportNote not found");
        }

        await EnsureReportAccessAsync(note.ReportId, user);
        if (entity.CreatorUserId != user.Id)
        {
            throw new AbpAuthorizationException("Only the creator can edit this comment");
        }

        entity.Text = input.Text;
        await _commentRepository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();

        var dto = ObjectMapper.Map<ReportNoteCommentDto>(entity);
        dto.CreatorUserId = entity.CreatorUserId;
        dto.CreatorFullName = $"{user.Name} {user.Surname}";
        dto.CreatorRoleName = (await UserManager.GetRolesAsync(user)).FirstOrDefault();
        return dto;
    }

    public async Task DeleteComment(EntityDto<Guid> input)
    {
        var user = await GetCurrentUserAsync();
        var entity = await _commentRepository.FirstOrDefaultAsync(input.Id);
        if (entity == null)
        {
            throw new EntityNotFoundException("ReportNoteComment not found");
        }

        var note = await _noteRepository.FirstOrDefaultAsync(entity.NoteId);
        if (note == null)
        {
            throw new EntityNotFoundException("ReportNote not found");
        }

        await EnsureReportAccessAsync(note.ReportId, user);
        if (entity.CreatorUserId != user.Id)
        {
            throw new AbpAuthorizationException("Only the creator can delete this comment");
        }

        await _commentRepository.DeleteAsync(entity);
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