using System.ComponentModel.DataAnnotations;

namespace HodHod.Maui.Models.Login;

public class ForgotPasswordModel
{
    [EmailAddress]
    [Required]
    public string EmailAddress { get; set; }
}