using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using HodHod.Authorization;
using HodHod.BlackLists.Dto;
using HodHod.BlackLists.Exporting;
using HodHod.Dto;
using HodHod.Storage;
using Microsoft.AspNetCore.Mvc;

namespace HodHod.BlackLists;

[AbpAuthorize]
public class BlackListAppService(
    IRepository<BlackListEntry, int> entryRepository,
    IBlackListExcelExporter excelExporter,
    ITempFileCacheManager tempFileCacheManager)
    : HodHodAppServiceBase, IBlackListAppService
{
    private readonly IRepository<BlackListEntry, int> _entryRepository = entryRepository;
    private readonly IBlackListExcelExporter _excelExporter = excelExporter;
    private readonly ITempFileCacheManager _tempFileCacheManager = tempFileCacheManager;

    public async Task<ListResultDto<BlackListEntryDto>> GetAll()
    {
        try
        {
            var list = await _entryRepository.GetAll().ToListAsync();
            var dtos = ObjectMapper.Map<List<BlackListEntryDto>>(list);
            return new ListResultDto<BlackListEntryDto>(dtos);
        }
        catch (Exception ex)
        {
            Logger.Error("خطا در GetAll: " + ex.Message, ex);
            throw;
        }
    }

    public async Task<BlackListEntryDto> Get(EntityDto<int> input)
    {
        try
        {
            var entity = await _entryRepository.GetAsync(input.Id);
            return ObjectMapper.Map<BlackListEntryDto>(entity);
        }
        catch (Exception ex)
        {
            Logger.Error($"خطا در Get ({input.Id}): " + ex.Message, ex);
            throw;
        }
    }

    public async Task<BlackListEntryDto> Create(CreateBlackListEntryDto input)
    {
        try
        {
            var entity = ObjectMapper.Map<BlackListEntry>(input);
            await _entryRepository.InsertAsync(entity);
            await CurrentUnitOfWork.SaveChangesAsync();
            return ObjectMapper.Map<BlackListEntryDto>(entity);
        }
        catch (Exception ex)
        {
            Logger.Error("خطا در Create: " + ex.Message, ex);
            throw;
        }
    }

    public async Task<BlackListEntryDto> Update(UpdateBlackListEntryDto input)
    {
        try
        {
            var entity = await _entryRepository.FirstOrDefaultAsync(input.Id);
            if (entity == null)
            {
                throw new EntityNotFoundException("BlackListEntry not found");
            }

            ObjectMapper.Map(input, entity);
            await _entryRepository.UpdateAsync(entity);
            await CurrentUnitOfWork.SaveChangesAsync();
            return ObjectMapper.Map<BlackListEntryDto>(entity);
        }
        catch (Exception ex)
        {
            Logger.Error($"خطا در Update ({input.Id}): " + ex.Message, ex);
            throw;
        }
    }

    public async Task Delete(EntityDto<int> input)
    {
        try
        {
            var entity = await _entryRepository.FirstOrDefaultAsync(input.Id);
            if (entity == null)
            {
                throw new EntityNotFoundException("BlackListEntry not found");
            }
            await _entryRepository.DeleteAsync(entity);
        }
        catch (Exception ex)
        {
            Logger.Error($"خطا در Delete ({input.Id}): " + ex.Message, ex);
            throw;
        }
    }

    public async Task<FileDto> GetListToExcel()
    {
        try
        {
            var list = await _entryRepository.GetAll().ToListAsync();
            var dtos = ObjectMapper.Map<List<BlackListEntryDto>>(list);
            return await _excelExporter.ExportToFile(dtos);
        }
        catch (Exception ex)
        {
            Logger.Error("خطا در GetListToExcel: " + ex.Message, ex);
            throw;
        }
    }

    [HttpGet]
    public async Task<FileContentResult> DownloadListToExcel()
    {
        var fileDto = await GetListToExcel();
        var bytes = _tempFileCacheManager.GetFile(fileDto.FileToken);
        _tempFileCacheManager.ClearFile(fileDto.FileToken);

        return new FileContentResult(bytes, fileDto.FileType)
        {
            FileDownloadName = fileDto.FileName
        };
    }
}
