using Abp.AutoMapper;
using HodHod.MultiTenancy;
using HodHod.MultiTenancy.Dto;
using HodHod.Web.Areas.App.Models.Common;

namespace HodHod.Web.Areas.App.Models.Tenants;

[AutoMapFrom(typeof(GetTenantFeaturesEditOutput))]
public class TenantFeaturesEditViewModel : GetTenantFeaturesEditOutput, IFeatureEditViewModel
{
    public Tenant Tenant { get; set; }
}

