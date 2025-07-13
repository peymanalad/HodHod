using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using HodHod.Common.Dto;
using HodHod.Editions.Dto;

namespace HodHod.Common;

public interface ICommonLookupAppService : IApplicationService
{
    Task<ListResultDto<SubscribableEditionComboboxItemDto>> GetEditionsForCombobox(bool onlyFreeItems = false);

    Task<PagedResultDto<FindUsersOutputDto>> FindUsers(FindUsersInput input);

    GetDefaultEditionNameOutput GetDefaultEditionName();
}

