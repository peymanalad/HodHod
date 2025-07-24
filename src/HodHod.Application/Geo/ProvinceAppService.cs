using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Domain.Repositories;
using HodHod.Authorization.Roles;
using HodHod.Authorization;
using Microsoft.EntityFrameworkCore;
using HodHod.Geo.Dto;

namespace HodHod.Geo;

[AbpAuthorize]
public class ProvinceAppService : HodHodAppServiceBase, IProvinceAppService
{
    private readonly IRepository<Province, int> _provinceRepository;
    private readonly IRepository<City, int> _cityRepository;

    public ProvinceAppService(IRepository<Province, int> provinceRepository, IRepository<City, int> cityRepository)
    {
        _provinceRepository = provinceRepository;
        _cityRepository = cityRepository;
    }

    public async Task<List<ProvinceDto>> GetProvincesAsync()
    {
        var user = await GetCurrentUserAsync();
        var roles = await UserManager.GetRolesAsync(user);

        IQueryable<Province> query = _provinceRepository.GetAll();
        if (roles.Contains(StaticRoleNames.Host.SuperAdmin) || roles.Contains(StaticRoleNames.Host.Admin))
        {
            // no filter
        }
        else if (roles.Contains(StaticRoleNames.Host.ProvinceAdmin))
        {
            if (!string.IsNullOrEmpty(user.Province))
            {
                query = query.Where(p => p.Name == user.Province);
            }
            else
            {
                return new List<ProvinceDto>();
            }
        }
        else if (roles.Contains(StaticRoleNames.Host.CityAdmin))
        {
            if (!string.IsNullOrEmpty(user.Province))
            {
                query = query.Where(p => p.Name == user.Province);
            }
            else
            {
                return new List<ProvinceDto>();
            }
        }
        else
        {
            throw new AbpAuthorizationException("Not authorized");
        }

        var provinces = await query.ToListAsync();
        return ObjectMapper.Map<List<ProvinceDto>>(provinces);
    }

    public async Task<List<CityDto>> GetCitiesAsync(int provinceId)
    {
        var user = await GetCurrentUserAsync();
        var roles = await UserManager.GetRolesAsync(user);

        IQueryable<City> query = _cityRepository.GetAll().Where(c => c.ProvinceId == provinceId);

        if (roles.Contains(StaticRoleNames.Host.SuperAdmin) || roles.Contains(StaticRoleNames.Host.Admin))
        {
            // no filter
        }
        else if (roles.Contains(StaticRoleNames.Host.ProvinceAdmin))
        {
            if (!string.IsNullOrEmpty(user.Province))
            {
                var province = await _provinceRepository.FirstOrDefaultAsync(p => p.Name == user.Province);
                if (province == null || province.Id != provinceId)
                {
                    return new List<CityDto>();
                }
            }
            else
            {
                return new List<CityDto>();
            }
        }
        else if (roles.Contains(StaticRoleNames.Host.CityAdmin))
        {
            if (!string.IsNullOrEmpty(user.Province) && !string.IsNullOrEmpty(user.City))
            {
                var province = await _provinceRepository.FirstOrDefaultAsync(p => p.Name == user.Province);
                if (province == null || province.Id != provinceId)
                {
                    return new List<CityDto>();
                }

                query = query.Where(c => c.Name == user.City);
            }
            else
            {
                return new List<CityDto>();
            }
        }
        else
        {
            throw new AbpAuthorizationException("Not authorized");
        }

        var cities = await query.ToListAsync();
        return ObjectMapper.Map<List<CityDto>>(cities);
    }
}