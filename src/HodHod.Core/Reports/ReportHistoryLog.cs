using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;

namespace HodHod.Reports;

[Table("AppReportHistoryLogs")]
public class ReportHistoryLog : FullAuditedEntity<Guid>
{
    public Guid ReportId { get; set; }

    public long PerformedByUserId { get; set; }

    [StringLength(128)]
    public string PerformedByFullName { get; set; }

    public ReportActionType ActionType { get; set; }

    public string ActionDetails { get; set; }

    public DateTime ActionTime { get; set; }

    public ReportHistoryVisibility Visibility { get; set; }
}