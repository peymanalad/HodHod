using System.Threading.Tasks;
using HodHod;
using HodHod.TokenAuth;
using HodHod.TokenAuth.Dto;

namespace HodHod.Auth;

public class AuthAppService : HodHodAppServiceBase, IAuthAppService
{
    private readonly ITokenAuthAppService _tokenAuthService;

    public AuthAppService(ITokenAuthAppService tokenAuthService)
    {
        _tokenAuthService = tokenAuthService;
    }

    public Task<AuthenticateResultModel> Authenticate(AuthenticateModel model)
    {
        return _tokenAuthService.Authenticate(model);
    }

    public Task<RefreshTokenResult> RefreshToken(string refreshToken)
    {
        return _tokenAuthService.RefreshToken(refreshToken);
    }

    public Task LogOut()
    {
        return _tokenAuthService.LogOut();
    }
}