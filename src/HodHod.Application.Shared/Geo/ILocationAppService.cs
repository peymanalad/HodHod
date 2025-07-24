using System.Threading.Tasks;
using Abp.Application.Services;
using HodHod.Geo.Dto;

namespace HodHod.Geo;

public interface ILocationAppService : IApplicationService
{
    Task<LocationResultDto> ReverseGeocodeAsync(double latitude, double longitude);
}