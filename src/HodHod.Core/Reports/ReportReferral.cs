using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;

namespace HodHod.Reports;

[Table("AppReportReferrals")]
public class ReportReferral : FullAuditedEntity<Guid>
{
    public Guid ReportId { get; set; }

    [ForeignKey(nameof(ReportId))]
    public Report Report { get; set; }

    public long FromUserId { get; set; }

    public long ToUserId { get; set; }

    [Required]
    [StringLength(2000)]
    public string Text { get; set; }

    public Guid? ParentId { get; set; }

    [ForeignKey(nameof(ParentId))]
    public ReportReferral Parent { get; set; }

    public ICollection<ReportReferral> Replies { get; set; }

    public ReportReferral()
    {
        Replies = new List<ReportReferral>();
    }
}