using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using HodHod.Geo.Dto;

namespace HodHod.Geo;

[AbpAllowAnonymous]
public class LocationAppService : HodHodAppServiceBase, ILocationAppService
{
    private readonly IGeocodingService _geocodingService;
    private readonly IRepository<Province, int> _provinceRepository;
    private readonly IRepository<City, int> _cityRepository;

    public LocationAppService(
        IGeocodingService geocodingService,
        IRepository<Province, int> provinceRepository,
        IRepository<City, int> cityRepository)
    {
        _geocodingService = geocodingService;
        _provinceRepository = provinceRepository;
        _cityRepository = cityRepository;
    }

    public async Task<LocationResultDto> ReverseGeocodeAsync(double latitude, double longitude)
    {
        var result = await _geocodingService.ReverseGeocodeAsync(latitude, longitude);
        var dto = ObjectMapper.Map<LocationResultDto>(result);

        var provinceName = TrimPrefix(dto.Province);
        var cityName = TrimPrefix(dto.City);

        if (!string.IsNullOrEmpty(provinceName))
        {
            var province = await _provinceRepository.FirstOrDefaultAsync(p => p.Name == provinceName);
            if (province != null)
            {
                dto.Province = province.Name;

                if (!string.IsNullOrEmpty(cityName))
                {
                    var city = await _cityRepository.FirstOrDefaultAsync(c => c.Name == cityName && c.ProvinceId == province.Id);
                    if (city != null)
                    {
                        dto.City = city.Name;
                    }
                }
            }
        }

        return dto;
    }

    private static string TrimPrefix(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return name;
        }

        name = name.Trim();
        if (name.StartsWith("استان "))
        {
            name = name.Substring(6);
        }
        if (name.StartsWith("شهرستان "))
        {
            name = name.Substring(8);
        }
        if (name.StartsWith("شهر "))
        {
            name = name.Substring(4);
        }

        return name;
    }
}