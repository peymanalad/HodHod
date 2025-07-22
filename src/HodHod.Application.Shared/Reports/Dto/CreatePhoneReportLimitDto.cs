using System.ComponentModel.DataAnnotations;

namespace HodHod.Reports.Dto;

public class CreatePhoneReportLimitDto
{
    [Required]
    public string PhoneNumber { get; set; }
    public int MaxFileCount { get; set; }
    public long MaxFileSizeInBytes { get; set; }
    public int MaxReportsPerHour { get; set; }
}