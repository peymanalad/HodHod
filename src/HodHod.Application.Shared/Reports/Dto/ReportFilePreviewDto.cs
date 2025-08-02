using System;

namespace HodHod.Reports.Dto;

public class ReportFilePreviewDto
{
    public string FileName { get; set; }
    public string FileType { get; set; }
    public string OriginalUrl { get; set; }
    public string PreviewUrl { get; set; }
}