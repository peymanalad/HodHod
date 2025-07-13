using Abp.AspNetCore.Mvc.Authorization;
using HodHod.Authorization.Users.Profile;
using HodHod.Storage;

namespace HodHod.Web.Controllers;

[AbpMvcAuthorize]
public class ProfileController : ProfileControllerBase
{
    public ProfileController(
        ITempFileCacheManager tempFileCacheManager,
        IProfileAppService profileAppService) :
        base(tempFileCacheManager, profileAppService)
    {
    }
}

