//using System;
//using System.Collections.Generic;
//using System.IdentityModel.Tokens.Jwt;
//using System.Linq;
//using System.Security.Claims;
//using System.Threading.Tasks;
//using Abp.Authorization;
//using Abp.Authorization.Users;
//using Abp.Extensions;
//using Abp.Runtime.Caching;
//using Abp.Runtime.Security;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.Extensions.Options;
//using Microsoft.IdentityModel.Tokens;
//using HodHod.Authorization;
//using HodHod.Authorization.PasswordlessLogin;
//using HodHod.Authorization.Roles;
//using HodHod.Authorization.Users;
//using HodHod.MultiTenancy;
//using HodHod.Net.Sms;
//using HodHod.Otp.Dto;
//using HodHod.Web.Authentication.JwtBearer;
//using HodHod.Authentication.PasswordlessLogin;
//using Abp.MultiTenancy;

//namespace HodHod.Otp;

//public class OtpAppService : HodHodAppServiceBase, IOtpAppService
//{
//    private readonly ISmsSender _smsSender;
//    private readonly IPasswordlessLoginManager _passwordlessLoginManager;
//    private readonly LogInManager _logInManager;
//    private readonly AbpLoginResultTypeHelper _abpLoginResultTypeHelper;
//    private readonly TokenAuthConfiguration _configuration;
//    private readonly UserManager _userManager;
//    private readonly ICacheManager _cacheManager;
//    private readonly IdentityOptions _identityOptions;
//    private readonly TenantManager _tenantManager;
//    public OtpAppService(
//        ISmsSender smsSender,
//        IPasswordlessLoginManager passwordlessLoginManager,
//        LogInManager logInManager,
//        AbpLoginResultTypeHelper abpLoginResultTypeHelper,
//        TokenAuthConfiguration configuration,
//        UserManager userManager,
//        ICacheManager cacheManager,
//        IOptions<IdentityOptions> identityOptions,
//        TenantManager tenantManager)
//    {
//        _smsSender = smsSender;
//        _passwordlessLoginManager = passwordlessLoginManager;
//        _logInManager = logInManager;
//        _abpLoginResultTypeHelper = abpLoginResultTypeHelper;
//        _configuration = configuration;
//        _userManager = userManager;
//        _cacheManager = cacheManager;
//        _identityOptions = identityOptions.Value;
//        _tenantManager = tenantManager;
//    }

//    public async Task SendOtpAsync(SendOtpInput input)
//    {
//        if (input.PhoneNumber.IsNullOrEmpty())
//        {
//            return;
//        }

//        var code = await _passwordlessLoginManager.GeneratePasswordlessLoginCode(
//            AbpSession.TenantId,
//            input.PhoneNumber
//        );

//        var message = string.Format(L("PasswordlessLogin_SmsMessage", HodHodConsts.ProductName, code));
//        await _smsSender.SendAsync(input.PhoneNumber, message);
//    }

//    public async Task<OtpLoginResultDto> LoginAsync(OtpLoginInput input)
//    {
//        if (input.Code.IsNullOrEmpty())
//        {
//            throw new AbpAuthorizationException("Verification code is required");
//        }

//        await _passwordlessLoginManager.VerifyPasswordlessLoginCode(
//            AbpSession.TenantId,
//            input.PhoneNumber,
//            input.Code
//        );

//        var user = await _passwordlessLoginManager.GetUserByPasswordlessProviderAndKeyAsync(
//            PasswordlessLoginProviderType.Sms.ToString(),
//            input.PhoneNumber
//        );

//        var loginResult = await GetPasswordlessLoginResultAsync(user);

//        var returnUrl = input.ReturnUrl;
//        if (input.SingleSignIn.HasValue && input.SingleSignIn.Value &&
//            loginResult.Result == AbpLoginResultType.Success)
//        {
//            loginResult.User.SetSignInToken();
//            returnUrl = AddSingleSignInParametersToReturnUrl(input.ReturnUrl, loginResult.User.SignInToken,
//                loginResult.User.Id, loginResult.User.TenantId);
//        }

//        var refreshToken = CreateRefreshToken(
//            await CreateJwtClaims(
//                loginResult.Identity,
//                loginResult.User,
//                tokenType: TokenType.RefreshToken
//            )
//        );

//        var accessToken = CreateAccessToken(
//            await CreateJwtClaims(
//                loginResult.Identity,
//                loginResult.User,
//                refreshTokenKey: refreshToken.key
//            )
//        );

//        await _passwordlessLoginManager.RemovePasswordlessLoginCode(
//            AbpSession.TenantId,
//            input.PhoneNumber
//        );

//        return new OtpLoginResultDto
//        {
//            AccessToken = accessToken,
//            ExpireInSeconds = (int)_configuration.AccessTokenExpiration.TotalSeconds,
//            RefreshToken = refreshToken.token,
//            RefreshTokenExpireInSeconds = (int)_configuration.RefreshTokenExpiration.TotalSeconds,
//            EncryptedAccessToken = GetEncryptedAccessToken(accessToken),
//            UserId = loginResult.User.Id,
//            ReturnUrl = returnUrl
//        };
//    }

//    private async Task<AbpLoginResult<Tenant, User>> GetPasswordlessLoginResultAsync(User user)
//    {
//        var loginResult = await _logInManager.CreateLoginResultAsync(user);

//        switch (loginResult.Result)
//        {
//            case AbpLoginResultType.Success:
//                return loginResult;
//            default:
//                throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(
//                    loginResult.Result,
//                    user.EmailAddress,
//                    await GetTenancyNameOrNull(user.TenantId)
//                );
//        }
//    }

//    private string CreateAccessToken(IEnumerable<Claim> claims, TimeSpan? expiration = null)
//    {
//        return CreateToken(claims, expiration ?? _configuration.AccessTokenExpiration);
//    }

//    private (string token, string key) CreateRefreshToken(IEnumerable<Claim> claims)
//    {
//        var claimsList = claims.ToList();
//        return (CreateToken(claimsList, AppConsts.RefreshTokenExpiration),
//            claimsList.First(c => c.Type == AppConsts.TokenValidityKey).Value);
//    }

//    private string CreateToken(IEnumerable<Claim> claims, TimeSpan? expiration = null)
//    {
//        var now = DateTime.UtcNow;

//        var jwtSecurityToken = new JwtSecurityToken(
//            issuer: _configuration.Issuer,
//            audience: _configuration.Audience,
//            claims: claims,
//            notBefore: now,
//            signingCredentials: _configuration.SigningCredentials,
//            expires: expiration == null ? (DateTime?)null : now.Add(expiration.Value)
//        );

//        return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
//    }

//    private static string GetEncryptedAccessToken(string accessToken)
//    {
//        return SimpleStringCipher.Instance.Encrypt(accessToken, AppConsts.DefaultPassPhrase);
//    }

//    private async Task<IEnumerable<Claim>> CreateJwtClaims(
//        ClaimsIdentity identity, User user,
//        TimeSpan? expiration = null,
//        TokenType tokenType = TokenType.AccessToken,
//        string refreshTokenKey = null)
//    {
//        var tokenValidityKey = Guid.NewGuid().ToString();
//        var claims = identity.Claims.ToList();
//        var nameIdClaim = claims.First(c => c.Type == _identityOptions.ClaimsIdentity.UserIdClaimType);

//        if (_identityOptions.ClaimsIdentity.UserIdClaimType != JwtRegisteredClaimNames.Sub)
//        {
//            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, nameIdClaim.Value));
//        }

//        claims.AddRange(new[]
//        {
//            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
//            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.Now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
//            new Claim(AppConsts.TokenValidityKey, tokenValidityKey),
//            new Claim(AppConsts.UserIdentifier, user.ToUserIdentifier().ToUserIdentifierString()),
//            new Claim(AppConsts.TokenType, ((int)tokenType).ToString())
//        });

//        if (!string.IsNullOrEmpty(refreshTokenKey))
//        {
//            claims.Add(new Claim(AppConsts.RefreshTokenValidityKey, refreshTokenKey));
//        }

//        if (!expiration.HasValue)
//        {
//            expiration = tokenType == TokenType.AccessToken
//                ? _configuration.AccessTokenExpiration
//                : _configuration.RefreshTokenExpiration;
//        }

//        var expirationDate = DateTime.UtcNow.Add(expiration.Value);

//        await _cacheManager
//            .GetCache(AppConsts.TokenValidityKey)
//            .SetAsync(tokenValidityKey, "", absoluteExpireTime: new DateTimeOffset(expirationDate));

//        await _userManager.AddTokenValidityKeyAsync(
//            user,
//            tokenValidityKey,
//            expirationDate
//        );

//        return claims;
//    }

//    private static string AddSingleSignInParametersToReturnUrl(string returnUrl, string signInToken, long userId, int? tenantId)
//    {
//        returnUrl += (returnUrl.Contains("?") ? "&" : "?") + "accessToken=" + signInToken +
//                     "&userId=" + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(userId.ToString()));
//        if (tenantId.HasValue)
//        {
//            returnUrl += "&tenantId=" + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(tenantId.Value.ToString()));
//        }

//        return returnUrl;
//    }

//    private async Task<string> GetTenancyNameOrNull(int? tenantId)
//    {
//        if (!tenantId.HasValue)
//            return null;

//        using (CurrentUnitOfWork.SetTenantId(null))
//        {
//            var tenant = await _tenantManager.GetByIdAsync(tenantId.Value);
//            return tenant?.TenancyName;
//        }
//    }


//}