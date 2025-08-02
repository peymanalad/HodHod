using System;

namespace HodHod.Reports.Dto;

public class ReportReferralDto
{
    public Guid Id { get; set; }
    public Guid ReportId { get; set; }
    public long SenderUserId { get; set; }
    public string SenderFullName { get; set; }
    public long ReceiverUserId { get; set; }
    public string ReceiverFullName { get; set; }
    public string Text { get; set; }

    public Guid? ParentId { get; set; }
    public DateTime CreationTime { get; set; }
}