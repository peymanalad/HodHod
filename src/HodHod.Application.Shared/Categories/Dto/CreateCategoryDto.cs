using System.ComponentModel.DataAnnotations;

namespace HodHod.Categories.Dto;

public class CreateCategoryDto
{
    [Required]
    [StringLength(256)]
    public string Name { get; set; }
}