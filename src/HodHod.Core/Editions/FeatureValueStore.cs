﻿using Abp.Application.Features;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.MultiTenancy;
using Abp.Runtime.Caching;
using HodHod.Authorization.Users;
using HodHod.MultiTenancy;

namespace HodHod.Editions;

public class FeatureValueStore : AbpFeatureValueStore<Tenant, User>
{
    public FeatureValueStore(
        ICacheManager cacheManager,
        IRepository<TenantFeatureSetting, long> tenantFeatureSettingRepository,
        IRepository<Tenant> tenantRepository,
        IRepository<EditionFeatureSetting, long> editionFeatureSettingRepository,
        IFeatureManager featureManager,
        IUnitOfWorkManager unitOfWorkManager)
        : base(cacheManager,
              tenantFeatureSettingRepository,
              tenantRepository,
              editionFeatureSettingRepository,
              featureManager,
              unitOfWorkManager)
    {
    }
}

