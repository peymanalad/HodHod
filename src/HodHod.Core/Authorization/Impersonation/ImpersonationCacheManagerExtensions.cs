﻿using Abp.Runtime.Caching;

namespace HodHod.Authorization.Impersonation;

public static class ImpersonationCacheManagerExtensions
{
    public static ITypedCache<string, ImpersonationCacheItem> GetImpersonationCache(this ICacheManager cacheManager)
    {
        return cacheManager.GetCache<string, ImpersonationCacheItem>(ImpersonationCacheItem.CacheName);
    }
}

