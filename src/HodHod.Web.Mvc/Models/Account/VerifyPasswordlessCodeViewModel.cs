using Abp.Localization;
using System.ComponentModel.DataAnnotations;

namespace HodHod.Web.Models.Account;

public class VerifyPasswordlessCodeViewModel
{
    [Required]
    [AbpDisplayName(HodHodConsts.LocalizationSourceName, "Code")]
    public string Code { get; set; }

    public string ProviderValue { get; set; }

    public string ProviderType { get; set; }

}

