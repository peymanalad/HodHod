using Abp.AutoMapper;
using HodHod.Sessions.Dto;

namespace HodHod.Web.Views.Shared.Components.TenantChange;

[AutoMapFrom(typeof(GetCurrentLoginInformationsOutput))]
public class TenantChangeViewModel
{
    public TenantLoginInfoDto Tenant { get; set; }
}

