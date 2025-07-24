using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using HodHod.Geo.Dto;

namespace HodHod.Geo;

public interface IProvinceAppService : IApplicationService
{
    Task<List<ProvinceDto>> GetProvincesAsync();
    Task<List<CityDto>> GetCitiesAsync(int provinceId);
}