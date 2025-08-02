using System;
using Abp.Application.Services.Dto;
using HodHod.Reports;

namespace HodHod.Reports.Dto;

public class ReportWithLastNoteDto : EntityDto<Guid>
{
    public string UniqueId { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; }
    public Guid SubCategoryId { get; set; }
    public string SubCategoryName { get; set; }
    public string LastNoteText { get; set; }
    public DateTime? LastNoteCreationTime { get; set; }
    public DateTime ReportCreationTime { get; set; }
    public long? LastNoteAuthorId { get; set; }
    //public string ReporterFullName { get; set; }
    public string LastNoteAuthorFullName { get; set; }
    public int NoteCount { get; set; }
    public ReportStatus Status { get; set; }
}