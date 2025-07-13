using System.Threading.Tasks;

namespace HodHod.Security;

public interface IPasswordComplexitySettingStore
{
    Task<PasswordComplexitySetting> GetSettingsAsync();
}

