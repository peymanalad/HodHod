﻿using Abp.Runtime.Caching;
using HodHod.Authentication.TwoFactor;

namespace HodHod.Web.Authentication.TwoFactor;

public static class TwoFactorCodeCacheExtensions
{
    public static ITypedCache<string, TwoFactorCodeCacheItem> GetTwoFactorCodeCache(this ICacheManager cacheManager)
    {
        return cacheManager.GetCache<string, TwoFactorCodeCacheItem>(TwoFactorCodeCacheItem.CacheName);
    }
}

