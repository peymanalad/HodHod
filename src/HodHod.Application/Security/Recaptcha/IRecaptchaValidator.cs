using System.Threading.Tasks;

namespace HodHod.Security.Recaptcha;

public interface IRecaptchaValidator
{
    Task ValidateAsync(string captchaResponse);
}
