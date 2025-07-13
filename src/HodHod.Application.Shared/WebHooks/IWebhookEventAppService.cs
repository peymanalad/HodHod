using System.Threading.Tasks;
using Abp.Webhooks;

namespace HodHod.WebHooks;

public interface IWebhookEventAppService
{
    Task<WebhookEvent> Get(string id);
}

