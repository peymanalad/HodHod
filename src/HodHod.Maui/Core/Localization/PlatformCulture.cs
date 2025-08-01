﻿namespace HodHod.Maui.Core.Localization;

/// <summary>
/// Helper class for splitting locales like
///   iOS: ms_MY, gsw_CH
///   Android: in-ID
/// into parts so we can create a .NET culture (or fallback culture)
/// </summary>
public class PlatformCulture
{
    public string PlatformString { get; }
    public string LanguageCode { get; }
    public string LocaleCode { get; }

    public PlatformCulture(string platformCultureString)
    {
        if (string.IsNullOrEmpty(platformCultureString))
        {
            throw new ArgumentException("Expected culture identifier", nameof(platformCultureString));
        }

        PlatformString = platformCultureString.Replace("_", "-"); // .NET expects dash, not underscore
        var dashIndex = PlatformString.IndexOf("-", StringComparison.Ordinal);
        if (dashIndex > 0)
        {
            var parts = PlatformString.Split('-');
            LanguageCode = parts[0];
            LocaleCode = parts[1];
        }
        else
        {
            LanguageCode = PlatformString;
            LocaleCode = "";
        }
    }

    public override string ToString()
    {
        return PlatformString;
    }
}