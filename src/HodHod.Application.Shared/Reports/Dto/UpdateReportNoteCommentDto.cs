using System;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;

namespace HodHod.Reports.Dto;

public class UpdateReportNoteCommentDto : EntityDto<Guid>
{
    [Required]
    [StringLength(2000)]
    public string Text { get; set; }
}