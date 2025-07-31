using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;

namespace HodHod.Reports;

[Table("AppReportNoteComments")]
public class ReportNoteComment : FullAuditedEntity<Guid>
{
    public Guid NoteId { get; set; }

    [ForeignKey(nameof(NoteId))]
    public ReportNote Note { get; set; }

    [Required]
    [StringLength(2000)]
    public string Text { get; set; }
}