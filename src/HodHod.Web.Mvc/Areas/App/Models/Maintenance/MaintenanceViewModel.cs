using System.Collections.Generic;
using HodHod.Caching.Dto;

namespace HodHod.Web.Areas.App.Models.Maintenance;

public class MaintenanceViewModel
{
    public IReadOnlyList<CacheDto> Caches { get; set; }

    public bool CanClearAllCaches { get; set; }
}

