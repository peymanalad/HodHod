using System;

namespace HodHod.Reports.Dto;

public class ReportNoteCommentDto
{
    public Guid Id { get; set; }
    public Guid NoteId { get; set; }
    public long? CreatorUserId { get; set; }
    public string CreatorUserName { get; set; }
    public DateTime CreationTime { get; set; }
    public string Text { get; set; }
}