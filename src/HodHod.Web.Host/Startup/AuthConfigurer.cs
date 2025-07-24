using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Runtime.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using HodHod.Web.Authentication.JwtBearer;

namespace HodHod.Web.Startup;

public static class AuthConfigurer
{
    public static void Configure(IServiceCollection services)
    {
        var authenticationBuilder = services.AddAuthentication();

        var isJwtEnabled = Environment.GetEnvironmentVariable("Authentication__JwtBearer__IsEnabled");
        if (bool.TryParse(isJwtEnabled, out var jwtEnabled) && jwtEnabled)
        {
            authenticationBuilder.AddAbpAsyncJwtBearer(options =>
            {
                var securityKey = Environment.GetEnvironmentVariable("Authentication__JwtBearer__SecurityKey");
                var issuer = Environment.GetEnvironmentVariable("Authentication__JwtBearer__Issuer");
                var audience = Environment.GetEnvironmentVariable("Authentication__JwtBearer__Audience");

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey)),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                options.AsyncSecurityTokenValidators.Clear();
                options.AsyncSecurityTokenValidators.Add(new HodHodAsyncJwtSecurityTokenHandler());

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = QueryStringTokenResolver
                };
            });
        }
    }

    private static Task QueryStringTokenResolver(MessageReceivedContext context)
    {
        if (!context.HttpContext.Request.Path.HasValue)
            return Task.CompletedTask;

        if (context.HttpContext.Request.Path.Value.StartsWith("/signalr"))
        {
            var allowAnonymous = Environment.GetEnvironmentVariable("App__AllowAnonymousSignalRConnection");
            return SetToken(context, bool.TryParse(allowAnonymous, out var result) && result);
        }

        List<string> urlsUsingEnchAuthToken = new()
        {
            "/Chat/GetUploadedObject?",
            "/Profile/GetProfilePictureByUser?"
        };

        if (urlsUsingEnchAuthToken.Any(url => context.HttpContext.Request.GetDisplayUrl().Contains(url)))
        {
            if (context.HttpContext.Request.Headers.ContainsKey("authorization"))
                return Task.CompletedTask;

            return SetToken(context, false);
        }

        return Task.CompletedTask;
    }

    private static Task SetToken(MessageReceivedContext context, bool allowAnonymous)
    {
        var qsAuthToken = context.HttpContext.Request.Query["enc_auth_token"].FirstOrDefault();
        if (qsAuthToken == null)
        {
            if (!allowAnonymous)
            {
                throw new AbpAuthorizationException("SignalR auth token is missing.");
            }

            return Task.CompletedTask;
        }

        context.Token = SimpleStringCipher.Instance.Decrypt(qsAuthToken, AppConsts.DefaultPassPhrase);
        return Task.CompletedTask;
    }
}
