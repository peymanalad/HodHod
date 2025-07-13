using System.ComponentModel.DataAnnotations;

namespace HodHod.Maui.Models.Login;

public class EmailActivationModel
{
    [Required]
    [EmailAddress]
    public string EmailAddress { get; set; }
}