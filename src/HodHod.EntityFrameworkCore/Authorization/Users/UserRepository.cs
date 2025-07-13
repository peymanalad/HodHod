﻿using System;
using System.Collections.Generic;
using Abp.EntityFrameworkCore;
using HodHod.EntityFrameworkCore;
using HodHod.EntityFrameworkCore.Repositories;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HodHod.Authorization.Users;

public class UserRepository : HodHodRepositoryBase<User, long>, IUserRepository
{
    public UserRepository(IDbContextProvider<HodHodDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public List<long> GetPasswordExpiredUserIds(DateTime passwordExpireDate)
    {
        var context = GetContext();

        return (
            from user in GetAll()
            let lastRecentPasswordOfUser = context.RecentPasswords
                .Where(rp => rp.UserId == user.Id && rp.TenantId == user.TenantId)
                .OrderByDescending(rp => rp.CreationTime).FirstOrDefault()
            where user.IsActive && !user.ShouldChangePasswordOnNextLogin &&
                  (
                      (lastRecentPasswordOfUser != null &&
                       lastRecentPasswordOfUser.CreationTime <= passwordExpireDate) ||
                      (lastRecentPasswordOfUser == null && user.CreationTime <= passwordExpireDate)
                  )
            select user.Id
        ).Distinct().ToList();
    }

    public async Task<User> FindByPhoneNumberAsync(string phoneNumber)
    {
        return await FirstOrDefaultAsync(user => user.PhoneNumber == phoneNumber);
    }

    public void UpdateUsersToChangePasswordOnNextLogin(List<long> userIdsToUpdate)
    {
        GetAll()
            .Where(user =>
                user.IsActive &&
                !user.ShouldChangePasswordOnNextLogin &&
                userIdsToUpdate.Contains(user.Id)
            )
            .ExecuteUpdate(setters => setters.SetProperty(b => b.ShouldChangePasswordOnNextLogin, true));
    }
}

