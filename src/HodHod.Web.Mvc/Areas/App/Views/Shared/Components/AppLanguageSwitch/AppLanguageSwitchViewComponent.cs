using System.Linq;
using System.Threading.Tasks;
using Abp.Localization;
using Microsoft.AspNetCore.Mvc;
using HodHod.Web.Areas.App.Models.Layout;
using HodHod.Web.Views;

namespace HodHod.Web.Areas.App.Views.Shared.Components.AppLanguageSwitch;

public class AppLanguageSwitchViewComponent : HodHodViewComponent
{
    private readonly ILanguageManager _languageManager;

    public AppLanguageSwitchViewComponent(ILanguageManager languageManager)
    {
        _languageManager = languageManager;
    }

    public Task<IViewComponentResult> InvokeAsync(string cssClass)
    {
        var model = new LanguageSwitchViewModel
        {
            Languages = _languageManager.GetActiveLanguages().ToList(),
            CurrentLanguage = _languageManager.CurrentLanguage,
            CssClass = cssClass
        };

        return Task.FromResult<IViewComponentResult>(View(model));
    }
}

