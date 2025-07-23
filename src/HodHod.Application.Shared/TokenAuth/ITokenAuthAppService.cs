using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using HodHod.TokenAuth.Dto;
using Microsoft.AspNetCore.Mvc;

namespace HodHod.TokenAuth;

public interface ITokenAuthAppService : IApplicationService
{
    Task<AuthenticateResultModel> Authenticate(AuthenticateModel model);

    Task AuthenticateWithQrCode(QrLoginAuthenticateModel model);

    Task<PasswordlessAuthenticateResultModel> PasswordlessAuthenticate(PasswordlessAuthenticateModel model);

    Task<RefreshTokenResult> RefreshToken(string refreshToken);

    Task LogOut();

    Task SendTwoFactorAuthCode(SendTwoFactorAuthCodeModel model);

    Task<ImpersonatedAuthenticateResultModel> ImpersonatedAuthenticate(string impersonationToken);

    Task<ImpersonatedAuthenticateResultModel> DelegatedImpersonatedAuthenticate(long userDelegationId, string password);

    Task<SwitchedAccountAuthenticateResultModel> LinkedAccountAuthenticate(string switchAccountToken);

    List<ExternalLoginProviderInfoModel> GetExternalAuthenticationProviders();

    Task<ExternalAuthenticateResultModel> ExternalAuthenticate(ExternalAuthenticateModel model);

    Task<string> TestNotification(string message = "", string severity = "info");
}

public class RefreshTokenResult
{
    public string AccessToken { get; }
    public string EncryptedAccessToken { get; }
    public int ExpireInSeconds { get; }

    public RefreshTokenResult(string accessToken, string encryptedAccessToken, int expireInSeconds)
    {
        AccessToken = accessToken;
        EncryptedAccessToken = encryptedAccessToken;
        ExpireInSeconds = expireInSeconds;
    }
}

public class QrLoginAuthenticateModel
{
    public string ConnectionId { get; set; }
    public string SessionId { get; set; }
}