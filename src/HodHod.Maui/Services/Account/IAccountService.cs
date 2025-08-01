﻿using HodHod.ApiClient.Models;

namespace HodHod.Maui.Services.Account;

public interface IAccountService
{
    AbpAuthenticateModel AbpAuthenticateModel { get; set; }

    AbpAuthenticateResultModel AuthenticateResultModel { get; set; }

    Task LoginUserAsync();

    Task LogoutAsync();
}