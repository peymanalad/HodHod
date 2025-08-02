using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using HodHod.Reports.Dto;

namespace HodHod.Reports;

public interface IReportReferralAppService : IApplicationService
{
    Task<ReportReferralDto> CreateAsync(CreateReportReferralDto input);
    Task<ReportReferralsDto> GetReferrals(Guid reportId);
    Task<ReportAssignableUsersDto> GetAssignableUsers(Guid reportId);
}