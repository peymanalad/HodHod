using System;
using System.ComponentModel.DataAnnotations;

namespace HodHod.Reports.Dto;

public class CreateReportNoteDto
{
    [Required]
    public Guid ReportId { get; set; }

    [Required]
    [StringLength(2000)]
    public string Text { get; set; }
}