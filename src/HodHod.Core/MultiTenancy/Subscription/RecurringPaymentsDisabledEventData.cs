﻿using Abp.Events.Bus;

namespace HodHod.MultiTenancy.Subscription;

public class RecurringPaymentsDisabledEventData : EventData
{
    public int TenantId { get; set; }

    public int DaysUntilDue { get; set; }

    public RecurringPaymentsDisabledEventData(int tenantId, int daysUntilDue)
    {
        TenantId = tenantId;
        DaysUntilDue = daysUntilDue;
    }
}

