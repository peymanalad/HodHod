using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Microsoft.EntityFrameworkCore;
using HodHod.Authorization;
using HodHod.Categories.Dto;

namespace HodHod.Categories;

[AbpAuthorize]
public class CategoryAppService : HodHodAppServiceBase, ICategoryAppService
{
    private readonly IRepository<Category, int> _categoryRepository;
    private readonly IRepository<SubCategory, int> _subCategoryRepository;

    public CategoryAppService(
        IRepository<Category, int> categoryRepository,
        IRepository<SubCategory, int> subCategoryRepository)
    {
        _categoryRepository = categoryRepository;
        _subCategoryRepository = subCategoryRepository;
    }

    [AbpAllowAnonymous]
    public async Task<ListResultDto<CategoryDto>> GetAll()
    {
        var categories = await _categoryRepository.GetAll()
            .Include(c => c.SubCategories)
            .ToListAsync();

        return new ListResultDto<CategoryDto>(
            ObjectMapper.Map<List<CategoryDto>>(categories));
    }

    [AbpAllowAnonymous]
    public async Task<CategoryDto> Get(EntityDto<int> input)
    {
        var category = await _categoryRepository.GetAll()
            .Include(c => c.SubCategories)
            .FirstAsync(c => c.Id == input.Id);
        return ObjectMapper.Map<CategoryDto>(category);
    }

    [AbpAllowAnonymous]
    public async Task<ListResultDto<SubCategoryDto>> GetSubCategories(EntityDto<int> input)
    {
        var subs = await _subCategoryRepository.GetAll()
            .Where(sc => sc.CategoryId == input.Id)
            .ToListAsync();
        return new ListResultDto<SubCategoryDto>(
            ObjectMapper.Map<List<SubCategoryDto>>(subs));
    }

    [AbpAuthorize(AppPermissions.Pages_Administration_Categories_Create)]
    public async Task<CategoryDto> Create(CreateCategoryDto input)
    {
        var category = ObjectMapper.Map<Category>(input);
        await _categoryRepository.InsertAsync(category);
        await CurrentUnitOfWork.SaveChangesAsync();
        return ObjectMapper.Map<CategoryDto>(category);
    }

    [AbpAuthorize(AppPermissions.Pages_Administration_Categories_Edit)]
    public async Task<CategoryDto> Update(UpdateCategoryDto input)
    {
        var category = await _categoryRepository.GetAsync(input.Id);
        ObjectMapper.Map(input, category);
        await _categoryRepository.UpdateAsync(category);
        return ObjectMapper.Map<CategoryDto>(category);
    }

    [AbpAuthorize(AppPermissions.Pages_Administration_Categories_Delete)]
    public async Task Delete(EntityDto<int> input)
    {
        await _categoryRepository.DeleteAsync(input.Id);
    }
}