﻿using Abp.Events.Bus;

namespace HodHod.MultiTenancy;

public class TenantEditionChangedEventData : EventData
{
    public int TenantId { get; set; }

    public int? OldEditionId { get; set; }

    public int? NewEditionId { get; set; }
}

