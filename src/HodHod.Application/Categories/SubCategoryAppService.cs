using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using HodHod.Authorization;
using HodHod.Categories.Dto;
using Abp.Domain.Entities;
using HodHod.Authorization.Roles;
using HodHod.BlackLists;
using Microsoft.AspNetCore.Http;
using Abp.UI;
using System.Linq;

namespace HodHod.Categories;

[AbpAuthorize]
public class SubCategoryAppService : HodHodAppServiceBase, ISubCategoryAppService
{
    private readonly IRepository<SubCategory, int> _subCategoryRepository;
    private readonly IRepository<Category, int> _categoryRepository;

    private readonly IRepository<BlackListEntry, int> _blackListRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SubCategoryAppService(
        IRepository<SubCategory, int> subCategoryRepository,
        IRepository<Category, int> categoryRepository,
        IRepository<BlackListEntry, int> blackListRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _subCategoryRepository = subCategoryRepository;
        _categoryRepository = categoryRepository;
        _blackListRepository = blackListRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    [AbpAllowAnonymous]
    public async Task<ListResultDto<SubCategoryDto>> GetAll()
    {
        await CheckBlackListFromHeaderAsync();
        var subs = await _subCategoryRepository.GetAllListAsync();
        return new ListResultDto<SubCategoryDto>(
            ObjectMapper.Map<List<SubCategoryDto>>(subs));
    }

    [AbpAllowAnonymous]
    public async Task<SubCategoryDto> Get(EntityDto<Guid> input)
    {
        await CheckBlackListFromHeaderAsync();
        var sub = await _subCategoryRepository
            .GetAll()
            .Include(s => s.Category)
            .FirstOrDefaultAsync(s => s.PublicId == input.Id);

        if (sub == null)
        {
            throw new EntityNotFoundException("SubCategory not found");
        }

        return ObjectMapper.Map<SubCategoryDto>(sub);
    }

    [AbpAuthorize(AppPermissions.Pages_Administration_SubCategories_Create)]
    public async Task<SubCategoryDto> Create(CreateSubCategoryDto input)
    {
        var currentUser = await GetCurrentUserAsync();
        if (!await UserManager.IsInRoleAsync(currentUser, StaticRoleNames.Host.SuperAdmin))
        {
            throw new AbpAuthorizationException("Only super admins can create subcategories.");
        }
        var category = await _categoryRepository
            .GetAll()
            .FirstOrDefaultAsync(c => c.PublicId == input.CategoryId);

        if (category == null)
        {
            throw new EntityNotFoundException("Category not found");
        }

        var sub = new SubCategory
        {
            Name = input.Name,
            CategoryId = category.Id,
            PublicId = Guid.NewGuid()
        };

        // فقط در صورت استفاده از s.Category.PublicId در mapping
        sub.Category = category;

        await _subCategoryRepository.InsertAsync(sub);
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<SubCategoryDto>(sub);
    }


    [AbpAuthorize(AppPermissions.Pages_Administration_SubCategories_Edit)]
    public async Task<SubCategoryDto> Update(UpdateSubCategoryDto input)
    {
        var currentUser = await GetCurrentUserAsync();
        if (!await UserManager.IsInRoleAsync(currentUser, StaticRoleNames.Host.SuperAdmin))
        {
            throw new AbpAuthorizationException("Only super admins can create subcategories.");
        }
        var sub = await _subCategoryRepository
            .GetAll()
            .FirstOrDefaultAsync(s => s.PublicId == input.PublicId);

        if (sub == null)
        {
            throw new EntityNotFoundException("SubCategory not found");
        }

        var category = await _categoryRepository
            .GetAll()
            .FirstOrDefaultAsync(c => c.PublicId == input.CategoryId);

        if (category == null)
        {
            throw new EntityNotFoundException("Category not found");
        }

        sub.Name = input.Name;
        sub.CategoryId = category.Id;
        sub.Category = category;

        await _subCategoryRepository.UpdateAsync(sub);
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<SubCategoryDto>(sub);
    }

    [AbpAuthorize(AppPermissions.Pages_Administration_SubCategories_Delete)]
    public async Task Delete(EntityDto<Guid> input)
    {
        var currentUser = await GetCurrentUserAsync();
        if (!await UserManager.IsInRoleAsync(currentUser, StaticRoleNames.Host.SuperAdmin))
        {
            throw new AbpAuthorizationException("Only super admins can create subcategories.");
        }
        var sub = await _subCategoryRepository
            .GetAll()
            .FirstOrDefaultAsync(s => s.PublicId == input.Id);

        if (sub == null)
        {
            throw new EntityNotFoundException("SubCategory not found");
        }

        await _subCategoryRepository.DeleteAsync(sub);
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