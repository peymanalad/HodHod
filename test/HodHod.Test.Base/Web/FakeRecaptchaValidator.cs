using System.Threading.Tasks;
using HodHod.Security.Recaptcha;

namespace HodHod.Test.Base.Web;

public class FakeRecaptchaValidator : IRecaptchaValidator
{
    public Task ValidateAsync(string captchaResponse)
    {
        return Task.CompletedTask;
    }
}
