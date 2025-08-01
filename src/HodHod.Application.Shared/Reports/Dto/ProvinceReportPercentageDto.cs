﻿using System.Collections.Generic;

namespace HodHod.Reports.Dto;

public class ProvinceReportPercentageDto
{
    public string Province { get; set; }
    public int TotalReports { get; set; }
    public double Percentage { get; set; }
    public string PercentageFormatted { get; set; }
    public List<CityReportPercentageDto> Cities { get; set; }
}