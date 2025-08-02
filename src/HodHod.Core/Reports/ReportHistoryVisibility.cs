using System;

namespace HodHod.Reports;

[Flags]
public enum ReportHistoryVisibility
{
    None = 0,
    SuperAdmin = 1,
    ProvinceAdmin = 2,
    CityAdmin = 4,
    PerformedUser = 8,
    All = SuperAdmin | ProvinceAdmin | CityAdmin
}