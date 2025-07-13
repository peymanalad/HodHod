using Abp.Domain.Uow;
using Abp.EntityFrameworkCore;
using Abp.OpenIddict.EntityFrameworkCore.Applications;
using HodHod.EntityFrameworkCore;

namespace HodHod.OpenIddict.Applications;

public class OpenIddictApplicationRepository : EfCoreOpenIddictApplicationRepository<HodHodDbContext>
{
    public OpenIddictApplicationRepository(
        IDbContextProvider<HodHodDbContext> dbContextProvider,
        IUnitOfWorkManager unitOfWorkManager) : base(dbContextProvider, unitOfWorkManager)
    {
    }
}

