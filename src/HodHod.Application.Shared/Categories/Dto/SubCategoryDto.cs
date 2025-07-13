using Abp.Application.Services.Dto;

namespace HodHod.Categories.Dto;

public class SubCategoryDto : EntityDto<int>
{
    public int CategoryId { get; set; }
    public string Name { get; set; }
}