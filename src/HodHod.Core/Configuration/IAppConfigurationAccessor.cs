using Microsoft.Extensions.Configuration;

namespace HodHod.Configuration;

public interface IAppConfigurationAccessor
{
    IConfigurationRoot Configuration { get; }
}

