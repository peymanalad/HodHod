﻿using System.Globalization;
using Abp.Dependency;

namespace HodHod.Localization;

public class ApplicationCulturesProvider : IApplicationCulturesProvider, ITransientDependency
{
    public CultureInfo[] GetAllCultures()
    {
        return CultureInfo.GetCultures(CultureTypes.AllCultures);
    }
}

