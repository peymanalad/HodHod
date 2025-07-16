﻿using System;

namespace HodHod.Authorization.Users;

[Serializable]
public class SwitchToLinkedAccountCacheItem
{
    public const string CacheName = "AppSwitchToLinkedAccountCache";

    public static readonly TimeSpan DefaultSlidingExpireTime = TimeSpan.FromMinutes(2);

    public int? TargetTenantId { get; set; }

    public long TargetUserId { get; set; }

    public int? ImpersonatorTenantId { get; set; }

    public long? ImpersonatorUserId { get; set; }

    public SwitchToLinkedAccountCacheItem()
    {

    }

    public SwitchToLinkedAccountCacheItem(
        int? targetTenantId,
        long targetUserId,
        int? impersonatorTenantId,
        long? impersonatorUserId
        )
    {
        TargetTenantId = targetTenantId;
        TargetUserId = targetUserId;
        ImpersonatorTenantId = impersonatorTenantId;
        ImpersonatorUserId = impersonatorUserId;
    }
}

