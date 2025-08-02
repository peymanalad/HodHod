using System.Collections.Generic;

namespace HodHod.Reports.Dto;

public class ReportReferralsDto
{
    public List<ReportReferralDto> All { get; set; } = new List<ReportReferralDto>();
    public List<ReportReferralDto> Inbox { get; set; } = new List<ReportReferralDto>();
    public List<ReportReferralDto> Outbox { get; set; } = new List<ReportReferralDto>();
}