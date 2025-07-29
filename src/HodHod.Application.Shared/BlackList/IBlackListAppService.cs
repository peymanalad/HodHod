using System.Threading.Tasks;
using System.Collections.Generic;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using HodHod.BlackLists.Dto;
using HodHod.Dto;

namespace HodHod.BlackLists;

public interface IBlackListAppService : IApplicationService
{
    Task<ListResultDto<BlackListEntryDto>> GetAll();
    Task<BlackListEntryDto> Get(EntityDto<int> input);
    Task<BlackListEntryDto> Create(CreateBlackListEntryDto input);
    Task<BlackListEntryDto> Update(UpdateBlackListEntryDto input);
    Task Delete(EntityDto<int> input);
    Task<FileDto> GetListToExcel();
}