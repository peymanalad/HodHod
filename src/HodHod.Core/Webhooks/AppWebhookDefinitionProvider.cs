﻿using Abp.Localization;
using Abp.Webhooks;
using HodHod.Webhooks;

namespace HodHod.WebHooks;

public class AppWebhookDefinitionProvider : WebhookDefinitionProvider
{
    public override void SetWebhooks(IWebhookDefinitionContext context)
    {
        context.Manager.Add(new WebhookDefinition(
            name: AppWebHookNames.TestWebhook
        ));

        //Add your webhook definitions here 
    }

    private static ILocalizableString L(string name)
    {
        return new LocalizableString(name, HodHodConsts.LocalizationSourceName);
    }
}

