using Abp.Domain.Services;

namespace HodHod.Authorization.Users.Password;

public interface IPasswordExpirationService : IDomainService
{
    void ForcePasswordExpiredUsersToChangeTheirPassword();
}

