﻿using System.Collections.Generic;
using Abp.AspNetZeroCore.Web.Authentication.External;
using Abp.AutoMapper;

namespace HodHod.TokenAuth.Dto;

[AutoMapFrom(typeof(ExternalLoginProviderInfo))]
public class ExternalLoginProviderInfoModel
{
    public string Name { get; set; }

    public string ClientId { get; set; }

    public Dictionary<string, string> AdditionalParams { get; set; }

}