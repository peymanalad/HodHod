﻿using System;
using System.Transactions;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.EntityFrameworkCore.Uow;
using Abp.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using HodHod.EntityFrameworkCore;
using HodHod.Migrations.Seed.Host;
using HodHod.Migrations.Seed.Tenants;
using Abp.Reflection.Extensions;
using HodHod.Configuration;
using Microsoft.Extensions.Configuration;

namespace HodHod.Migrations.Seed;

public static class SeedHelper
{
    public static void SeedHostDb(IIocResolver iocResolver)
    {
        WithDbContext<HodHodDbContext>(iocResolver, SeedHostDb);
    }

    public static void SeedHostDb(HodHodDbContext context)
    {
        context.SuppressAutoSetTenantId = true;
        var configuration = AppConfigurations.Get(typeof(SeedHelper).Assembly.GetDirectoryPathOrNull());
        var dbConn = ConnectionStringProvider.Get(configuration);
        new InitialHostDbBuilder(context).Create();

        //Default tenant seed (in host database).
        new DefaultTenantBuilder(context).Create();
        new TenantRoleAndUserBuilder(context, 1).Create();
    }

    private static void WithDbContext<TDbContext>(IIocResolver iocResolver, Action<TDbContext> contextAction)
        where TDbContext : DbContext
    {
        using (var uowManager = iocResolver.ResolveAsDisposable<IUnitOfWorkManager>())
        {
            using (var uow = uowManager.Object.Begin(TransactionScopeOption.Suppress))
            {
                var context = uowManager.Object.Current.GetDbContext<TDbContext>(MultiTenancySides.Host);

                contextAction(context);

                uow.Complete();
            }
        }
    }
}

