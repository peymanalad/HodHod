using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using HodHod.Categories.Dto;
using System.Collections.Generic;

namespace HodHod.Categories;

public interface ICategoryAppService : IApplicationService
{
    Task<ListResultDto<CategoryDto>> GetAll();
    Task<CategoryDto> Get(EntityDto<int> input);
    Task<ListResultDto<SubCategoryDto>> GetSubCategories(EntityDto<int> input);
    Task<CategoryDto> Create(CreateCategoryDto input);
    Task<CategoryDto> Update(UpdateCategoryDto input);
    Task Delete(EntityDto<int> input);
}