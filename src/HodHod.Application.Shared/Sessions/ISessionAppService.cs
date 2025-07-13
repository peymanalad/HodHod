using System.Threading.Tasks;
using Abp.Application.Services;
using HodHod.Sessions.Dto;

namespace HodHod.Sessions;

public interface ISessionAppService : IApplicationService
{
    Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();

    Task<UpdateUserSignInTokenOutput> UpdateUserSignInToken();
}

