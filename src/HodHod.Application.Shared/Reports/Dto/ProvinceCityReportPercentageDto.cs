namespace HodHod.Reports.Dto;

using System.Collections.Generic;

public class ProvinceCityReportPercentageDto
{
    public string Province { get; set; }

    public int TotalReports { get; set; }

    public double Percentage { get; set; }

    public string PercentageFormatted { get; set; }

    public List<CityReportPercentageDto> Cities { get; set; }
}