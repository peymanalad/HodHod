using Abp.Domain.Services;

namespace HodHod;

public abstract class HodHodDomainServiceBase : DomainService
{
    /* Add your common members for all your domain services. */

    protected HodHodDomainServiceBase()
    {
        LocalizationSourceName = HodHodConsts.LocalizationSourceName;
    }
}

