using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using HodHod.Caching.Dto;

namespace HodHod.Caching;

public interface ICachingAppService : IApplicationService
{
    ListResultDto<CacheDto> GetAllCaches();

    Task ClearCache(EntityDto<string> input);

    Task ClearAllCaches();

    bool CanClearAllCaches();
}

