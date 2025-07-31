using System;
using System.ComponentModel.DataAnnotations;

namespace HodHod.Reports.Dto;

public class CreateReportNoteCommentDto
{
    [Required]
    public Guid NoteId { get; set; }

    [Required]
    [StringLength(2000)]
    public string Text { get; set; }
}