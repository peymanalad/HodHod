using System;

namespace HodHod.Authorization.PasswordlessLogin;

[Serializable]
public class OtpSendLimitCacheItem
{
    public const string CacheName = "AppOtpSendLimitCache";

    public static readonly TimeSpan DefaultSlidingExpireTime = TimeSpan.FromHours(1);

    public DateTime WindowStart { get; set; }

    public int Count { get; set; }

    public OtpSendLimitCacheItem()
    {
    }

    public OtpSendLimitCacheItem(DateTime windowStart, int count)
    {
        WindowStart = windowStart;
        Count = count;
    }
}