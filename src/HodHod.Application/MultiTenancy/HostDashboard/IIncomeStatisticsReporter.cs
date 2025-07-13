using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HodHod.MultiTenancy.HostDashboard.Dto;

namespace HodHod.MultiTenancy.HostDashboard;

public interface IIncomeStatisticsService
{
    Task<List<IncomeStastistic>> GetIncomeStatisticsData(DateTime startDate, DateTime endDate,
        ChartDateInterval dateInterval);
}
