using System;
using System.Collections.Generic;
using Abp.Application.Services.Dto;

namespace HodHod.Reports.Dto;

public class ReportDto : EntityDto<long>
{
    public int CategoryId { get; set; }
    public int SubCategoryId { get; set; }
    public string Description { get; set; }
    public string Address { get; set; }
    public double? Longitude { get; set; }
    public double? Latitude { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime CreationTime { get; set; }
    public List<string> FilePaths { get; set; }
}