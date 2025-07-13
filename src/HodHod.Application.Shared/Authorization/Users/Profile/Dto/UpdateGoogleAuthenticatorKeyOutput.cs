using System.Collections.Generic;

namespace HodHod.Authorization.Users.Profile.Dto;

public class UpdateGoogleAuthenticatorKeyOutput
{
    public IEnumerable<string> RecoveryCodes { get; set; }
}

