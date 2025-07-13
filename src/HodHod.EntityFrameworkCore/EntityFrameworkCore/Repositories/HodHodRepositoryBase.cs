using Abp.Domain.Entities;
using Abp.EntityFrameworkCore;
using Abp.EntityFrameworkCore.Repositories;

namespace HodHod.EntityFrameworkCore.Repositories;

/// <summary>
/// Base class for custom repositories of the application.
/// </summary>
/// <typeparam name="TEntity">Entity type</typeparam>
/// <typeparam name="TPrimaryKey">Primary key type of the entity</typeparam>
public abstract class HodHodRepositoryBase<TEntity, TPrimaryKey> : EfCoreRepositoryBase<HodHodDbContext, TEntity, TPrimaryKey>
    where TEntity : class, IEntity<TPrimaryKey>
{
    protected HodHodRepositoryBase(IDbContextProvider<HodHodDbContext> dbContextProvider)
        : base(dbContextProvider)
    {

    }

    //add your common methods for all repositories
}