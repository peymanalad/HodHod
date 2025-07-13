using Abp.Configuration;
using Abp.Net.Mail;
using Abp.Net.Mail.Smtp;
using Abp.Runtime.Security;

namespace HodHod.Net.Emailing;

public class HodHodSmtpEmailSenderConfiguration : SmtpEmailSenderConfiguration
{
    public HodHodSmtpEmailSenderConfiguration(ISettingManager settingManager) : base(settingManager)
    {

    }

    public override string Password => SimpleStringCipher.Instance.Decrypt(GetNotEmptySettingValue(EmailSettingNames.Smtp.Password));
}

