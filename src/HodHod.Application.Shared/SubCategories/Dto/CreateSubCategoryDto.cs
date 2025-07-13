using System.ComponentModel.DataAnnotations;

namespace HodHod.Categories.Dto;

public class CreateSubCategoryDto
{
    public int CategoryId { get; set; }

    [Required]
    [StringLength(256)]
    public string Name { get; set; }
}