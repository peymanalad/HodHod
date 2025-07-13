using System.Collections.Generic;
using Abp.Application.Services.Dto;

namespace HodHod.Categories.Dto;

public class CategoryDto : EntityDto<int>
{
    public string Name { get; set; }
    public List<SubCategoryDto> SubCategories { get; set; }
}