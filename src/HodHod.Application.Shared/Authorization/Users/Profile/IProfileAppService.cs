﻿using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using HodHod.Authorization.Users.Dto;
using HodHod.Authorization.Users.Profile.Dto;
using HodHod.Dto;

namespace HodHod.Authorization.Users.Profile;

public interface IProfileAppService : IApplicationService
{
    Task<CurrentUserProfileEditDto> GetCurrentUserProfileForEdit();

    Task UpdateCurrentUserProfile(CurrentUserProfileEditDto input);

    Task ChangePassword(ChangePasswordInput input);

    Task UpdateProfilePicture(UpdateProfilePictureInput input);

    Task<GetPasswordComplexitySettingOutput> GetPasswordComplexitySetting();

    Task<GetProfilePictureOutput> GetProfilePicture();

    Task<GetProfilePictureOutput> GetProfilePictureByUser(long userId);

    Task<GetProfilePictureOutput> GetProfilePictureByUserName(string username);

    Task<GetProfilePictureOutput> GetFriendProfilePicture(GetFriendProfilePictureInput input);

    Task ChangeLanguage(ChangeUserLanguageDto input);

    Task SendVerificationSms(SendVerificationSmsInputDto input);

    Task VerifySmsCode(VerifySmsCodeInputDto input);

    Task PrepareCollectedData();

    Task<GenerateGoogleAuthenticatorKeyOutput> GenerateGoogleAuthenticatorKey();

    Task<UpdateGoogleAuthenticatorKeyOutput> UpdateGoogleAuthenticatorKey(UpdateGoogleAuthenticatorKeyInput input);

    Task<bool> VerifyAuthenticatorCode(VerifyAuthenticatorCodeInput input);

    Task DisableGoogleAuthenticator(VerifyAuthenticatorCodeInput input);

    Task<UpdateGoogleAuthenticatorKeyOutput> ViewRecoveryCodes(VerifyAuthenticatorCodeInput input);
}

