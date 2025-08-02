using System;
using System.ComponentModel.DataAnnotations;

namespace HodHod.Reports.Dto;

public class CreateReportReferralDto
{
    [Required]
    public Guid ReportId { get; set; }
    [Required]
    public string UniqueId { get; set; }

    [Required]
    public long ReceiverUserId { get; set; }

    [Required]
    [StringLength(2000)]
    public string Text { get; set; }

    public Guid? ParentId { get; set; }
}