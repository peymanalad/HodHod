using System.Collections.Generic;

namespace HodHod.TokenAuth.Dto;

public class AuthenticateResultModel
{
    public string AccessToken { get; set; }

    public string EncryptedAccessToken { get; set; }

    public int ExpireInSeconds { get; set; }

    public bool ShouldResetPassword { get; set; }

    public string PasswordResetCode { get; set; }

    public long UserId { get; set; }

    public bool RequiresTwoFactorVerification { get; set; }

    public IList<string> TwoFactorAuthProviders { get; set; }

    public string TwoFactorRememberClientToken { get; set; }
    public int RefreshTokenExpireInSeconds { get; set; }

    public string ReturnUrl { get; set; }

    public string RefreshToken { get; set; }
    public string c { get; set; }
    public IList<string> Roles { get; set; }
    public string FullName { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}