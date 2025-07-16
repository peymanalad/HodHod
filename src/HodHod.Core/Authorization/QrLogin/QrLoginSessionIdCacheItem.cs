﻿using System;

namespace HodHod.Authorization.QrLogin;

public class QrLoginSessionIdCacheItem
{
    public const string CacheName = "AppQrLoginSessionIdCache";

    public string Code { get; set; }

    public static readonly TimeSpan DefaultSlidingExpireTime = TimeSpan.FromMinutes(2);

    public QrLoginSessionIdCacheItem(string code)
    {
        Code = code;
    }
}

