using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using HodHod.Reports.Dto;

namespace HodHod.Reports;

public interface IReportNoteCommentAppService : IApplicationService
{
    Task<List<ReportNoteCommentDto>> GetComments(Guid noteId);
    Task<ReportNoteCommentDto> AddComment(CreateReportNoteCommentDto input);
    Task<ReportNoteCommentDto> UpdateComment(UpdateReportNoteCommentDto input);
    Task DeleteComment(EntityDto<Guid> input);
}