﻿using Abp.Runtime.Caching;

namespace HodHod.Authorization.Users.Profile.Cache;

public static class SmsVerificationCodeCacheExtensions
{
    public static ITypedCache<string, SmsVerificationCodeCacheItem> GetSmsVerificationCodeCache(this ICacheManager cacheManager)
    {
        return cacheManager.GetCache<string, SmsVerificationCodeCacheItem>(SmsVerificationCodeCacheItem.CacheName);
    }
}
