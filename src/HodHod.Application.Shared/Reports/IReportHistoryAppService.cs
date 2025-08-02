using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using System.Collections.Generic;
using HodHod.Reports.Dto;

namespace HodHod.Reports;

public interface IReportHistoryAppService : IApplicationService
{
    Task<List<ReportHistoryLogDto>> GetHistory(Guid reportId);
}