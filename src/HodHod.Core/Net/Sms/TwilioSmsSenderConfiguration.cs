using System;
using Abp.Dependency;
using Abp.Extensions;
using Microsoft.Extensions.Configuration;
using HodHod.Configuration;

namespace HodHod.Net.Sms;

public class TwilioSmsSenderConfiguration : ITransientDependency
{
    private readonly IConfigurationRoot _appConfiguration;

    public string AccountSid => Environment.GetEnvironmentVariable("Twilio:AccountSid");

    public string AuthToken => Environment.GetEnvironmentVariable("Twilio:AuthToken");

    public string SenderNumber => Environment.GetEnvironmentVariable("Twilio:SenderNumber");

    public TwilioSmsSenderConfiguration(IAppConfigurationAccessor configurationAccessor)
    {
        _appConfiguration = configurationAccessor.Configuration;
    }
}

