using System.Threading.Tasks;
using HodHod.Sessions.Dto;

namespace HodHod.Web.Session;

public interface IPerRequestSessionCache
{
    Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformationsAsync();
}

