using Abp.Zero.Ldap.Authentication;
using Abp.Zero.Ldap.Configuration;
using HodHod.Authorization.Users;
using HodHod.MultiTenancy;

namespace HodHod.Authorization.Ldap;

public class AppLdapAuthenticationSource : LdapAuthenticationSource<Tenant, User>
{
    public AppLdapAuthenticationSource(ILdapSettings settings, IAbpZeroLdapModuleConfig ldapModuleConfig)
        : base(settings, ldapModuleConfig)
    {
    }
}

