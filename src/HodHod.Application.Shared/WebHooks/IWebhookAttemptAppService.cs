﻿using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using HodHod.WebHooks.Dto;

namespace HodHod.WebHooks;

public interface IWebhookAttemptAppService
{
    Task<PagedResultDto<GetAllSendAttemptsOutput>> GetAllSendAttempts(GetAllSendAttemptsInput input);

    Task<ListResultDto<GetAllSendAttemptsOfWebhookEventOutput>> GetAllSendAttemptsOfWebhookEvent(GetAllSendAttemptsOfWebhookEventInput input);
}

