using Abp.Domain.Uow;
using Abp.EntityFrameworkCore;
using Abp.OpenIddict.EntityFrameworkCore.Authorizations;
using HodHod.EntityFrameworkCore;

namespace HodHod.OpenIddict.Authorizations;

public class OpenIddictAuthorizationRepository : EfCoreOpenIddictAuthorizationRepository<HodHodDbContext>
{
    public OpenIddictAuthorizationRepository(
        IDbContextProvider<HodHodDbContext> dbContextProvider,
        IUnitOfWorkManager unitOfWorkManager) : base(dbContextProvider, unitOfWorkManager)
    {
    }
}

