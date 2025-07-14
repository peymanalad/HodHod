using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HodHod.Reports.Dto;

public class CreateReportDto
{
    [Required]
    public int CategoryId { get; set; }

    [Required]
    public int SubCategoryId { get; set; }

    [Required]
    [StringLength(4096)]
    public string Description { get; set; }

    [StringLength(1024)]
    public string Address { get; set; }

    public double? Longitude { get; set; }

    public double? Latitude { get; set; }

    [Required]
    public string PhoneNumber { get; set; }

    [Required]
    public string OtpCode { get; set; }

    public List<string> FileTokens { get; set; }
}