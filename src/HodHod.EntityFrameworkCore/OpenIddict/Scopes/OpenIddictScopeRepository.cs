using Abp.Domain.Uow;
using Abp.EntityFrameworkCore;
using Abp.OpenIddict.EntityFrameworkCore.Scopes;
using HodHod.EntityFrameworkCore;

namespace HodHod.OpenIddict.Scopes;

public class OpenIddictScopeRepository : EfCoreOpenIddictScopeRepository<HodHodDbContext>
{
    public OpenIddictScopeRepository(
        IDbContextProvider<HodHodDbContext> dbContextProvider,
        IUnitOfWorkManager unitOfWorkManager) : base(dbContextProvider, unitOfWorkManager)
    {
    }
}

