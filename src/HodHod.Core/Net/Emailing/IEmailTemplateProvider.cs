﻿namespace HodHod.Net.Emailing;

public interface IEmailTemplateProvider
{
    string GetDefaultTemplate(int? tenantId);
}

