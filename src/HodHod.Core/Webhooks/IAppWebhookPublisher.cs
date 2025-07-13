using System.Threading.Tasks;
using HodHod.Authorization.Users;

namespace HodHod.WebHooks;

public interface IAppWebhookPublisher
{
    Task PublishTestWebhook();
}

