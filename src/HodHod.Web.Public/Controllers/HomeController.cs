using Microsoft.AspNetCore.Mvc;
using HodHod.Web.Controllers;

namespace HodHod.Web.Public.Controllers;

public class HomeController : HodHodControllerBase
{
    public ActionResult Index()
    {
        return View();
    }
}

