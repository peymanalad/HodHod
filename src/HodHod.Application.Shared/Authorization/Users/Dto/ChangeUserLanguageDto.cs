using System.ComponentModel.DataAnnotations;

namespace HodHod.Authorization.Users.Dto;

public class ChangeUserLanguageDto
{
    [Required]
    public string LanguageName { get; set; }
}

