using System.Threading.Tasks;
using System.Collections.Generic;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using HodHod.Reports.Dto;

namespace HodHod.Reports;

public interface IPhoneReportLimitAppService : IApplicationService
{
    Task<ListResultDto<PhoneReportLimitDto>> GetAll();
    Task<PhoneReportLimitDto> Get(EntityDto<int> input);
    Task<PhoneReportLimitDto> Create(CreatePhoneReportLimitDto input);
    Task<PhoneReportLimitDto> Update(UpdatePhoneReportLimitDto input);
    Task Delete(EntityDto<int> input);
}