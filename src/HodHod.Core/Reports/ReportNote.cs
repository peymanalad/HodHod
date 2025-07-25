using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;

namespace HodHod.Reports;

[Table("AppReportNotes")]
public class ReportNote : FullAuditedEntity<Guid>
{
    public Guid ReportId { get; set; }

    [ForeignKey(nameof(ReportId))]
    public Report Report { get; set; }

    [Required]
    [StringLength(2000)]
    public string Text { get; set; }
}