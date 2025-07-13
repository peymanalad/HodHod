using Abp.Auditing;
using HodHod.Configuration.Dto;

namespace HodHod.Configuration.Tenants.Dto;

public class TenantEmailSettingsEditDto : EmailSettingsEditDto
{
    public bool UseHostDefaultEmailSettings { get; set; }
}

