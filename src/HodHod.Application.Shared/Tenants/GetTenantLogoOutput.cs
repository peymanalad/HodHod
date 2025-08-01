﻿using Abp.Extensions;

namespace HodHod.Tenants;

public class GetTenantLogoOutput
{
    public string Logo { get; set; }

    public string LogoFileType { get; set; }

    public bool HasLogo => !Logo.IsNullOrWhiteSpace() && !LogoFileType.IsNullOrWhiteSpace();

    public GetTenantLogoOutput()
    {

    }

    public GetTenantLogoOutput(string profilePicture, string logoFileType)
    {
        Logo = profilePicture;
        LogoFileType = logoFileType;
    }
}

