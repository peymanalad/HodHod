using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using HodHod.Web.Controllers;

namespace HodHod.Web.Areas.App.Controllers;

[Area("App")]
[AbpMvcAuthorize]
public class WelcomeController : HodHodControllerBase
{
    public ActionResult Index()
    {
        return View();
    }
}

