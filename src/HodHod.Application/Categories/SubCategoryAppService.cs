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

namespace HodHod.Categories;

[AbpAuthorize]
public class SubCategoryAppService : HodHodAppServiceBase, ISubCategoryAppService
{
    private readonly IRepository<SubCategory, int> _subCategoryRepository;
    private readonly IRepository<Category, int> _categoryRepository;

    public SubCategoryAppService(IRepository<SubCategory, int> subCategoryRepository, IRepository<Category, int> categoryRepository)
    {
        _subCategoryRepository = subCategoryRepository;
        _categoryRepository = categoryRepository;
    }

    [AbpAllowAnonymous]
    public async Task<ListResultDto<SubCategoryDto>> GetAll()
    {
        var subs = await _subCategoryRepository.GetAllListAsync();
        return new ListResultDto<SubCategoryDto>(
            ObjectMapper.Map<List<SubCategoryDto>>(subs));
    }

    [AbpAllowAnonymous]
    public async Task<SubCategoryDto> Get(EntityDto<Guid> input)
    {
        var sub = await _subCategoryRepository
            .GetAll()
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

        await _subCategoryRepository.UpdateAsync(sub);

        return ObjectMapper.Map<SubCategoryDto>(sub);
    }

    [AbpAuthorize(AppPermissions.Pages_Administration_SubCategories_Delete)]
    public async Task Delete(EntityDto<Guid> input)
    {
        var sub = await _subCategoryRepository
            .GetAll()
            .FirstOrDefaultAsync(s => s.PublicId == input.Id);

        if (sub == null)
        {
            throw new EntityNotFoundException("SubCategory not found");
        }

        await _subCategoryRepository.DeleteAsync(sub);
    }
}