namespace HodHod.Reports.Dto;

public class CityReportPercentageDto
{
    public string City { get; set; }

    public int TotalReports { get; set; }

    public double Percentage { get; set; }

    public string PercentageFormatted { get; set; }
}