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
using HodHod.BlackLists;
using Microsoft.AspNetCore.Http;
using Abp.UI;

namespace HodHod.Categories;

[AbpAuthorize]
public class CategoryAppService : HodHodAppServiceBase, ICategoryAppService
{
    private readonly IRepository<Category, int> _categoryRepository;
    private readonly IRepository<SubCategory, int> _subCategoryRepository;
    private readonly IRepository<BlackListEntry, int> _blackListRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CategoryAppService(
        IRepository<Category, int> categoryRepository,
        IRepository<SubCategory, int> subCategoryRepository,
        IRepository<BlackListEntry, int> blackListRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _categoryRepository = categoryRepository;
        _subCategoryRepository = subCategoryRepository;
        _blackListRepository = blackListRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    [AbpAllowAnonymous]
    public async Task<ListResultDto<CategoryDto>> GetAll()
    {
        await CheckBlackListFromHeaderAsync();
        var categories = await _categoryRepository.GetAll()
            .Include(c => c.SubCategories)
            .ToListAsync();

        return new ListResultDto<CategoryDto>(
            ObjectMapper.Map<List<CategoryDto>>(categories));
    }

    [AbpAllowAnonymous]
    public async Task<CategoryDto> Get(EntityDto<Guid> input)
    {
        await CheckBlackListFromHeaderAsync();
        var category = await _categoryRepository.GetAll()
            .Include(c => c.SubCategories)
            .FirstAsync(c => c.PublicId == input.Id);
        return ObjectMapper.Map<CategoryDto>(category);
    }

    [AbpAllowAnonymous]
    public async Task<ListResultDto<SubCategoryDto>> GetSubCategories(EntityDto<Guid> input)
    {
        await CheckBlackListFromHeaderAsync();
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

    private async Task CheckBlackListFromHeaderAsync()
    {
        var phone = _httpContextAccessor.HttpContext?.Request?.Headers["X-PhoneNumber"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(phone))
        {
            return;
        }

        var normalized = PhoneNumberHelper.Normalize(phone);
        if (long.TryParse(normalized, out var digits))
        {
            if (await _blackListRepository.CountAsync(e => e.PhoneNumber == digits) > 0)
            {
                throw new UserFriendlyException(L("PhoneNumberBlackListed"));
            }
        }
    }
}