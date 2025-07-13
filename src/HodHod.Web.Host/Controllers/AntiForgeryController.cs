using Microsoft.AspNetCore.Antiforgery;

namespace HodHod.Web.Controllers;

public class AntiForgeryController : HodHodControllerBase
{
    private readonly IAntiforgery _antiforgery;

    public AntiForgeryController(IAntiforgery antiforgery)
    {
        _antiforgery = antiforgery;
    }

    public void GetToken()
    {
        _antiforgery.SetCookieTokenAndHeader(HttpContext);
    }
}

