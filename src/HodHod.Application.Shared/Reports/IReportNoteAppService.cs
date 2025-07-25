using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using HodHod.Reports.Dto;

namespace HodHod.Reports;

public interface IReportNoteAppService : IApplicationService
{
    Task<List<ReportNoteDto>> GetNotes(Guid reportId);
    Task<ReportNoteDto> AddNote(CreateReportNoteDto input);
    Task<ReportNoteDto> UpdateNote(UpdateReportNoteDto input);
    Task DeleteNote(EntityDto<Guid> input);
}