using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using HodHod.Categories.Dto;
using System.Collections.Generic;

namespace HodHod.Categories;

public interface ISubCategoryAppService : IApplicationService
{
    Task<ListResultDto<SubCategoryDto>> GetAll();
    Task<SubCategoryDto> Get(EntityDto<int> input);
    Task<SubCategoryDto> Create(CreateSubCategoryDto input);
    Task<SubCategoryDto> Update(UpdateSubCategoryDto input);
    Task Delete(EntityDto<int> input);
}