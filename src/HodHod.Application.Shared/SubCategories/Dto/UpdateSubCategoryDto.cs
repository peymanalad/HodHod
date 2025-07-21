using System;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;

namespace HodHod.Categories.Dto;

public class UpdateSubCategoryDto
{
    public Guid PublicId { get; set; }
    public Guid CategoryId { get; set; }

    [Required]
    [StringLength(256)]
    public string Name { get; set; }
}