using System;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;

namespace HodHod.Categories.Dto;

public class UpdateCategoryDto : EntityDto<int>
{
    public Guid PublicId { get; set; }
    [Required]
    [StringLength(256)]
    public string Name { get; set; }
}