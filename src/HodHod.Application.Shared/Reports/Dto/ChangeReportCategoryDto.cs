using System;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;

namespace HodHod.Reports.Dto;

public class ChangeReportCategoryDto : EntityDto<Guid>
{
    [Required]
    public Guid CategoryId { get; set; }

    [Required]
    public Guid SubCategoryId { get; set; }
}