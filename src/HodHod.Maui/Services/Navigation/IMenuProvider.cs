using HodHod.Maui.Models.NavigationMenu;

namespace HodHod.Maui.Services.Navigation;

public interface IMenuProvider
{
    List<NavigationMenuItem> GetAuthorizedMenuItems(Dictionary<string, string> grantedPermissions);
}