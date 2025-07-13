using System.Collections.Generic;

namespace HodHod.Configuration.Dto;

public class ExternalLoginSettingsDto
{
    public List<string> EnabledSocialLoginSettings { get; set; } = new List<string>();
}

