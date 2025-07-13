using System.ComponentModel.DataAnnotations;

namespace HodHod.Localization.Dto;

public class CreateOrUpdateLanguageInput
{
    [Required]
    public ApplicationLanguageEditDto Language { get; set; }
}

