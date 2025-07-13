using Abp.Dependency;
using HodHod.Configuration;
using HodHod.Url;

namespace HodHod.Web.Url;

public class WebUrlService : WebUrlServiceBase, IWebUrlService, ITransientDependency
{
    public WebUrlService(
        IAppConfigurationAccessor configurationAccessor) :
        base(configurationAccessor)
    {
    }

    public override string WebSiteRootAddressFormatKey => "App:ClientRootAddress";

    public override string ServerRootAddressFormatKey => "App:ServerRootAddress";
}

