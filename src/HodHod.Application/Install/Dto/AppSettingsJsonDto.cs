﻿using System.Collections.Generic;
using Abp;

namespace HodHod.Install.Dto;

public class AppSettingsJsonDto
{
    public string WebSiteUrl { get; set; }

    public string ServerSiteUrl { get; set; }

    public List<NameValue> Languages { get; set; }
}
