﻿using System;
using System.Collections.Generic;

namespace HodHod.MultiTenancy.HostDashboard.Dto;

public class GetRecentTenantsOutput
{
    public int RecentTenantsDayCount { get; set; }

    public int MaxRecentTenantsShownCount { get; set; }

    public DateTime TenantCreationStartDate { get; set; }

    public List<RecentTenant> RecentTenants { get; set; }
}

