﻿namespace HodHod.Web.Areas.App.Models.Common.Modals;

public class ModalHeaderViewModel
{
    public string Title { get; set; }

    public ModalHeaderViewModel(string title)
    {
        Title = title;
    }
}

