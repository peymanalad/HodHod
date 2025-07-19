using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using HodHod.Storage;

namespace HodHod.Reports;

[Table("AppReportFiles")]
public class ReportFile : CreationAuditedEntity<Guid>
{
    public Guid ReportId { get; set; }

    [ForeignKey(nameof(ReportId))]
    public Report Report { get; set; }

    [StringLength(256)]
    public string FileName { get; set; }
    [Required]
    [StringLength(1024)]
    public string FilePath { get; set; }
}