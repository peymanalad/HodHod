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
using System;
using Abp.Domain.Entities;
using HodHod.Authorization.Roles;

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
    public async Task<CategoryDto> Get(EntityDto<Guid> input)
    {
        var category = await _categoryRepository.GetAll()
            .Include(c => c.SubCategories)
            .FirstAsync(c => c.PublicId == input.Id);
        return ObjectMapper.Map<CategoryDto>(category);
    }

    [AbpAllowAnonymous]
    public async Task<ListResultDto<SubCategoryDto>> GetSubCategories(EntityDto<Guid> input)
    {
        var category = await _categoryRepository
            .GetAll()
            .FirstOrDefaultAsync(c => c.PublicId == input.Id);
        var subs = await _subCategoryRepository.GetAll()
            .Include(sc => sc.Category)
            .Where(sc => sc.CategoryId == category.Id)
            .ToListAsync();
        return new ListResultDto<SubCategoryDto>(
            ObjectMapper.Map<List<SubCategoryDto>>(subs));
    }

    [AbpAuthorize(AppPermissions.Pages_Administration_Categories_Create)]
    public async Task<CategoryDto> Create(CreateCategoryDto input)
    {
        var currentUser = await GetCurrentUserAsync();
        if (!await UserManager.IsInRoleAsync(currentUser, StaticRoleNames.Host.SuperAdmin))
        {
            throw new AbpAuthorizationException("Only super admins can create categories.");
        }
        var category = ObjectMapper.Map<Category>(input);
        category.PublicId = Guid.NewGuid();
        await _categoryRepository.InsertAsync(category);
        await CurrentUnitOfWork.SaveChangesAsync();
        return ObjectMapper.Map<CategoryDto>(category);
    }

    [AbpAuthorize(AppPermissions.Pages_Administration_Categories_Edit)]
    public async Task<CategoryDto> Update(UpdateCategoryDto input)
    {
        var currentUser = await GetCurrentUserAsync();
        if (!await UserManager.IsInRoleAsync(currentUser, StaticRoleNames.Host.SuperAdmin))
        {
            throw new AbpAuthorizationException("Only super admins can create categories.");
        }
        var category = await _categoryRepository
            .GetAll()
            .FirstOrDefaultAsync(c => c.PublicId == input.PublicId);

        if (category == null)
        {
            throw new EntityNotFoundException("Category not found");
        }

        category.Name = input.Name;
        await _categoryRepository.UpdateAsync(category);
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<CategoryDto>(category);
    }


    [AbpAuthorize(AppPermissions.Pages_Administration_Categories_Delete)]
    public async Task Delete(EntityDto<Guid> input)
    {
        var currentUser = await GetCurrentUserAsync();
        if (!await UserManager.IsInRoleAsync(currentUser, StaticRoleNames.Host.SuperAdmin))
        {
            throw new AbpAuthorizationException("Only super admins can create categories.");
        }
        var category = await _categoryRepository
            .GetAll()
            .FirstOrDefaultAsync(c => c.PublicId == input.Id);

        if (category == null)
        {
            throw new EntityNotFoundException("Category not found");
        }

        await _categoryRepository.DeleteAsync(category);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

}