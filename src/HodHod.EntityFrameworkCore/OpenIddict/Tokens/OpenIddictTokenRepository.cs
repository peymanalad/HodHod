using Abp.Domain.Uow;
using Abp.EntityFrameworkCore;
using Abp.OpenIddict.EntityFrameworkCore.Tokens;
using HodHod.EntityFrameworkCore;

namespace HodHod.OpenIddict.Tokens;

public class OpenIddictTokenRepository : EfCoreOpenIddictTokenRepository<HodHodDbContext>
{
    public OpenIddictTokenRepository(
        IDbContextProvider<HodHodDbContext> dbContextProvider,
        IUnitOfWorkManager unitOfWorkManager) : base(dbContextProvider, unitOfWorkManager)
    {
    }
}

