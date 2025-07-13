using Microsoft.AspNetCore.Components;
using HodHod.Maui.Core.Components;
using HodHod.Maui.Core.Threading;
using HodHod.Maui.Services.UI;


namespace HodHod.Maui.Pages.MySettings;

public partial class ThemeSwitch : HodHodComponentBase
{
    private string _selectedTheme = ThemeService.GetUserTheme();

    private string[] _themes = ThemeService.GetAllThemes();

    public string SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            _selectedTheme = value;
            AsyncRunner.Run(ThemeService.SetUserTheme(JS, SelectedTheme));
        }
    }
}