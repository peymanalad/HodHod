using System.ComponentModel.DataAnnotations;
using Abp.Localization;

namespace HodHod.Web.Models.Account;

public class VerifySecurityCodeViewModel
{
    [Required]
    public string Provider { get; set; }

    [Required]
    [AbpDisplayName(HodHodConsts.LocalizationSourceName, "Code")]
    public string Code { get; set; }

    public string ReturnUrl { get; set; }

    [AbpDisplayName(HodHodConsts.LocalizationSourceName, "RememberThisBrowser")]
    public bool RememberBrowser { get; set; }

    public bool RememberMe { get; set; }

    public bool IsRememberBrowserEnabled { get; set; }
}

