using HodHod.Maui.Services.Navigation;
using HodHod.Maui.Services.Permission;
using HodHod.Maui.Services.UI;

namespace HodHod.Maui.Core.Components;

public class HodHodMainLayoutPageComponentBase : HodHodComponentBase
{
    protected PageHeaderService PageHeaderService { get; set; }

    protected DomManipulatorService DomManipulatorService { get; set; }

    protected INavigationService NavigationService { get; set; }

    protected IPermissionService PermissionService { get; set; }

    public HodHodMainLayoutPageComponentBase()
    {
        PageHeaderService = Resolve<PageHeaderService>();
        DomManipulatorService = Resolve<DomManipulatorService>();
        NavigationService = Resolve<INavigationService>();
        PermissionService = Resolve<IPermissionService>();
    }

    protected async Task SetPageHeader(string title)
    {
        PageHeaderService.Title = title;
        PageHeaderService.SubTitle = string.Empty;
        PageHeaderService.ClearButton();
        await DomManipulatorService.ClearModalBackdrop(JS);
    }

    protected async Task SetPageHeader(string title, string subTitle)
    {
        PageHeaderService.Title = title;
        PageHeaderService.SubTitle = subTitle;
        PageHeaderService.ClearButton();
        await DomManipulatorService.ClearModalBackdrop(JS);
    }

    protected async Task SetPageHeader(string title, string subTitle, List<PageHeaderButton> buttons)
    {
        PageHeaderService.Title = title;
        PageHeaderService.SubTitle = subTitle;
        PageHeaderService.SetButtons(buttons);
        await DomManipulatorService.ClearModalBackdrop(JS);
    }
}