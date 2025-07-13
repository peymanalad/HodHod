using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace HodHod.Web.Authentication.JwtBearer;

public class AsyncJwtBearerOptions : JwtBearerOptions
{
    public readonly List<IAsyncSecurityTokenValidator> AsyncSecurityTokenValidators;

    private readonly HodHodAsyncJwtSecurityTokenHandler _defaultAsyncHandler = new HodHodAsyncJwtSecurityTokenHandler();

    public AsyncJwtBearerOptions()
    {
        AsyncSecurityTokenValidators = new List<IAsyncSecurityTokenValidator>() { _defaultAsyncHandler };
    }
}


