﻿using Abp.Localization;

namespace HodHod.Web.Areas.App.Models.Languages;

public class EditTextModalViewModel
{
    public string SourceName { get; set; }

    public LanguageInfo BaseLanguage { get; set; }

    public LanguageInfo TargetLanguage { get; set; }

    public string BaseText { get; set; }

    public string TargetText { get; set; }

    public string Key { get; set; }
}

