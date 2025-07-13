using Microsoft.JSInterop;
using HodHod.ApiClient;
using HodHod.Maui.Core.Components;
using HodHod.Maui.Models.NavigationMenu;
using HodHod.Maui.Services.Navigation;
using HodHod.Maui.Services.UI;
using HodHod.Maui.Services.User;

namespace HodHod.Maui.Pages.Layout;

public partial class NavMenu : HodHodComponentBase
{
    protected IMenuProvider MenuProvider { get; set; }
    protected IApplicationContext ApplicationContext { get; set; }
    protected IAccessTokenManager AccessTokenManager { get; set; }
    protected LanguageService LanguageService { get; set; }
    protected IUserProfileService UserProfileService { get; set; }

    protected List<NavigationMenuItem> MenuItems;

    private bool HasUserInfo => AccessTokenManager != null &&
                                AccessTokenManager.IsUserLoggedIn &&
                                ApplicationContext != null &&
                                ApplicationContext.LoginInfo != null &&
                                ApplicationContext?.LoginInfo?.User != null;

    private string _userImage;

    protected override async Task OnInitializedAsync()
    {
        MenuItems = new List<NavigationMenuItem>();

        MenuProvider = Resolve<IMenuProvider>();
        ApplicationContext = Resolve<IApplicationContext>();
        AccessTokenManager = Resolve<IAccessTokenManager>();
        UserProfileService = Resolve<IUserProfileService>();

        LanguageService = Resolve<LanguageService>();
        LanguageService.OnLanguageChanged += (s, e) =>
        {
            BuildMenuItems();
            StateHasChanged();
        };

        BuildMenuItems();
        await GetUserPhoto();
    }

    public void BuildMenuItems()
    {
        if (!AccessTokenManager.IsUserLoggedIn)
        {
            MenuItems = MenuProvider.GetAuthorizedMenuItems(null);
            return;
        }

        var grantedPermissions = ApplicationContext.Configuration.Auth.GrantedPermissions;
        MenuItems = MenuProvider.GetAuthorizedMenuItems(grantedPermissions);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Task.Delay(1000);
            await base.JS.InvokeVoidAsync("KTDialer.init");
            await base.JS.InvokeVoidAsync("KTDrawer.init");
            await base.JS.InvokeVoidAsync("KTImageInput.init");
            await base.JS.InvokeVoidAsync("KTMenu.init");
            await base.JS.InvokeVoidAsync("KTPasswordMeter.init");
            await base.JS.InvokeVoidAsync("KTScroll.init");
            await base.JS.InvokeVoidAsync("KTScrolltop.init");
            await base.JS.InvokeVoidAsync("KTSticky.init");
            await base.JS.InvokeVoidAsync("KTSwapper.init");
            await base.JS.InvokeVoidAsync("KTToggle.init");
            await base.JS.InvokeVoidAsync("KTApp.init");
            await base.JS.InvokeVoidAsync("KTAppLayoutBuilder.init");
            await base.JS.InvokeVoidAsync("KTLayoutSearch.init");
            await base.JS.InvokeVoidAsync("KTAppSidebar.init");
            await base.JS.InvokeVoidAsync("KTThemeModeUser.init");
            await base.JS.InvokeVoidAsync("KTThemeMode.init");
            await base.JS.InvokeVoidAsync("KTLayoutToolbar.init");
        }
    }

    private async Task GetUserPhoto()
    {
        if (!HasUserInfo)
        {
            return;
        }

        _userImage = await UserProfileService.GetProfilePicture(ApplicationContext.LoginInfo.User.Id);
        StateHasChanged();
    }
}