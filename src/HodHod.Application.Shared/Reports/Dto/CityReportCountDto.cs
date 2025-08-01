namespace HodHod.Reports.Dto;

public class CityReportCountDto
{
    public string CityName { get; set; }
    public int TotalReports { get; set; }
    public double Percentage { get; set; }
    public string PercentageFormatted { get; set; }
}