using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using HodHod.Authorization;
using HodHod.Categories.Dto;

namespace HodHod.Categories;

[AbpAuthorize]
public class SubCategoryAppService : HodHodAppServiceBase, ISubCategoryAppService
{
    private readonly IRepository<SubCategory, int> _subCategoryRepository;

    public SubCategoryAppService(IRepository<SubCategory, int> subCategoryRepository)
    {
        _subCategoryRepository = subCategoryRepository;
    }

    [AbpAllowAnonymous]
    public async Task<ListResultDto<SubCategoryDto>> GetAll()
    {
        var subs = await _subCategoryRepository.GetAllListAsync();
        return new ListResultDto<SubCategoryDto>(
            ObjectMapper.Map<List<SubCategoryDto>>(subs));
    }

    [AbpAllowAnonymous]
    public async Task<SubCategoryDto> Get(EntityDto<int> input)
    {
        var sub = await _subCategoryRepository.GetAsync(input.Id);
        return ObjectMapper.Map<SubCategoryDto>(sub);
    }

    [AbpAuthorize(AppPermissions.Pages_Administration_SubCategories_Create)]
    public async Task<SubCategoryDto> Create(CreateSubCategoryDto input)
    {
        var sub = ObjectMapper.Map<SubCategory>(input);
        await _subCategoryRepository.InsertAsync(sub);
        await CurrentUnitOfWork.SaveChangesAsync();
        return ObjectMapper.Map<SubCategoryDto>(sub);
    }

    [AbpAuthorize(AppPermissions.Pages_Administration_SubCategories_Edit)]
    public async Task<SubCategoryDto> Update(UpdateSubCategoryDto input)
    {
        var sub = await _subCategoryRepository.GetAsync(input.Id);
        ObjectMapper.Map(input, sub);
        await _subCategoryRepository.UpdateAsync(sub);
        return ObjectMapper.Map<SubCategoryDto>(sub);
    }

    [AbpAuthorize(AppPermissions.Pages_Administration_SubCategories_Delete)]
    public async Task Delete(EntityDto<int> input)
    {
        await _subCategoryRepository.DeleteAsync(input.Id);
    }
}