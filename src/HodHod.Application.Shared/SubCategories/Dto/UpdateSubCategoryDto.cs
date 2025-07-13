using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;

namespace HodHod.Categories.Dto;

public class UpdateSubCategoryDto : EntityDto<int>
{
    public int CategoryId { get; set; }

    [Required]
    [StringLength(256)]
    public string Name { get; set; }
}