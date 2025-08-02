using System;
using Abp.Application.Services.Dto;
using HodHod.Reports;

namespace HodHod.Reports.Dto;

public class ReportHistoryLogDto : EntityDto<Guid>
{
    public Guid ReportId { get; set; }
    public long PerformedByUserId { get; set; }
    public string PerformedByFullName { get; set; }
    public ReportActionType ActionType { get; set; }
    public string ActionDetails { get; set; }
    public DateTime ActionTime { get; set; }
    public ReportHistoryVisibility Visibility { get; set; }
}