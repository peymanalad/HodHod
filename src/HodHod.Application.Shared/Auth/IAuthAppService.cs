using System.Threading.Tasks;
using Abp.Application.Services;
using HodHod.TokenAuth;
using HodHod.TokenAuth.Dto;

namespace HodHod.Auth;

public interface IAuthAppService : IApplicationService
{
    Task<AuthenticateResultModel> Authenticate(AuthenticateModel model);

    Task<RefreshTokenResult> RefreshToken(string refreshToken);

    Task LogOut();
}