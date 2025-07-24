using System.Threading.Tasks;
using Abp.Dependency;

namespace HodHod.Geo;

public interface IGeocodingService : ITransientDependency
{
    Task<LocationResult> ReverseGeocodeAsync(double latitude, double longitude);
}