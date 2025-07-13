﻿using HodHod.Maui.Core;
using HodHod.Maui.Services.UI;

namespace HodHod.Maui.Pages.Layout;

public partial class PageHeaderComponent
{
    protected PageHeaderService PageHeaderService { get; set; }

    public PageHeaderComponent()
    {
        PageHeaderService = DependencyResolver.Resolve<PageHeaderService>();
        PageHeaderService.TitleChanged += (s, e) => StateHasChanged();
        PageHeaderService.SubTitleChanged += (s, e) => StateHasChanged();
        PageHeaderService.HeaderButtonChanged += (s, e) => StateHasChanged();
    }

    public async Task HandleButtonOnClick(HeaderButtonInfo HeaderButtonInfo)
    {
        if (HeaderButtonInfo == null)
        {
            return;
        }

        await HeaderButtonInfo.OnClick();
    }
}