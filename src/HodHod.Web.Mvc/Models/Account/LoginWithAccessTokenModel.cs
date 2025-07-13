using System.ComponentModel.DataAnnotations;
using Abp.Auditing;

namespace HodHod.Web.Models.Account;

public class LoginWithAccessTokenModel
{
    public string AccessToken { get; set; }
}

