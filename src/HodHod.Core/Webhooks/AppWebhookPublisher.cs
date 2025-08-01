﻿using System;
using System.Threading.Tasks;
using Abp.Webhooks;
using HodHod.Webhooks;

namespace HodHod.WebHooks;

public class AppWebhookPublisher : HodHodDomainServiceBase, IAppWebhookPublisher
{
    private readonly IWebhookPublisher _webHookPublisher;

    public AppWebhookPublisher(IWebhookPublisher webHookPublisher)
    {
        _webHookPublisher = webHookPublisher;
    }

    public async Task PublishTestWebhook()
    {
        var separator = DateTime.Now.Millisecond;
        await _webHookPublisher.PublishAsync(AppWebHookNames.TestWebhook,
            new
            {
                UserName = "Test Name " + separator,
                EmailAddress = "Test Email " + separator
            }
        );
    }
}

