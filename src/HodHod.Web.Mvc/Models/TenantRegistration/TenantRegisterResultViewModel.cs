using Abp.AutoMapper;
using HodHod.MultiTenancy.Dto;

namespace HodHod.Web.Models.TenantRegistration;

[AutoMapFrom(typeof(RegisterTenantOutput))]
public class TenantRegisterResultViewModel : RegisterTenantOutput
{
    public string TenantLoginAddress { get; set; }
}

