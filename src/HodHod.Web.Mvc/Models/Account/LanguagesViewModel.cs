﻿using System.Collections.Generic;
using Abp.Localization;

namespace HodHod.Web.Models.Account;

public class LanguagesViewModel
{
    public LanguageInfo CurrentLanguage { get; set; }

    public IReadOnlyList<LanguageInfo> AllLanguages { get; set; }
}

