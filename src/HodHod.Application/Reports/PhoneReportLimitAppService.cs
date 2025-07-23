using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using HodHod.Authorization;
using HodHod.Reports.Dto;
using System;

namespace HodHod.Reports;

[AbpAuthorize(AppPermissions.Pages_Administration_PhoneReportLimits)]
public class PhoneReportLimitAppService : HodHodAppServiceBase, IPhoneReportLimitAppService
{
    private readonly IRepository<PhoneReportLimit, int> _limitRepository;

    public PhoneReportLimitAppService(IRepository<PhoneReportLimit, int> limitRepository)
    {
        _limitRepository = limitRepository;
    }

    public async Task<ListResultDto<PhoneReportLimitDto>> GetAll()
    {
        try
        {
            var limits = await _limitRepository.GetAllListAsync();
            var dtoList = ObjectMapper.Map<List<PhoneReportLimitDto>>(limits);
            return new ListResultDto<PhoneReportLimitDto>(dtoList);
        }
        catch (Exception ex)
        {
            Logger.Error("Error in GetAll of PhoneReportLimit", ex); // اگر Logger داری
            throw; // یا یک Exception قابل فهم‌تر برگردون
        }
    }


    public async Task<PhoneReportLimitDto> Get(EntityDto<int> input)
    {
        var entity = await _limitRepository.GetAsync(input.Id);
        return ObjectMapper.Map<PhoneReportLimitDto>(entity);
    }

    public async Task<PhoneReportLimitDto> Create(CreatePhoneReportLimitDto input)
    {
        var entity = ObjectMapper.Map<PhoneReportLimit>(input);
        await _limitRepository.InsertAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
        return ObjectMapper.Map<PhoneReportLimitDto>(entity);
    }

    public async Task<PhoneReportLimitDto> Update(UpdatePhoneReportLimitDto input)
    {
        var entity = await _limitRepository.GetAsync(input.Id);
        ObjectMapper.Map(input, entity);
        await _limitRepository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
        return ObjectMapper.Map<PhoneReportLimitDto>(entity);
    }

    public async Task Delete(EntityDto<int> input)
    {
        var entity = await _limitRepository.FirstOrDefaultAsync(input.Id);
        if (entity == null)
        {
            throw new EntityNotFoundException("PhoneReportLimit not found");
        }
        await _limitRepository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }
}