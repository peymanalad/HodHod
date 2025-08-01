﻿using System.Collections.Generic;

namespace HodHod.Url;

public interface IWebUrlService
{
    string WebSiteRootAddressFormat { get; }

    string ServerRootAddressFormat { get; }

    bool SupportsTenancyNameInUrl { get; }

    string GetSiteRootAddress(string tenancyName = null);

    string GetServerRootAddress(string tenancyName = null);

    List<string> GetRedirectAllowedExternalWebSites();
}

