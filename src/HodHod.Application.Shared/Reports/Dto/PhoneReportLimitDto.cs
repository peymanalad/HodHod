namespace HodHod.Reports.Dto;

public class PhoneReportLimitDto
{
    public int Id { get; set; }
    public string PhoneNumber { get; set; }
    public int MaxFileCount { get; set; }
    public long MaxFileSizeInBytes { get; set; }
    public int MaxReportsPerHour { get; set; }
}