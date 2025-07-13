using Abp;
using System.Threading.Tasks;

namespace HodHod.Authorization.Users.DataCleaners;

public interface IUserDataCleaner
{
    Task CleanUserData(UserIdentifier userIdentifier);
}

