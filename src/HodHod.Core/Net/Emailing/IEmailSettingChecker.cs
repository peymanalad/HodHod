using System.Threading.Tasks;

namespace HodHod.Net.Emailing;

public interface IEmailSettingsChecker
{
    bool EmailSettingsValid();

    Task<bool> EmailSettingsValidAsync();
}

