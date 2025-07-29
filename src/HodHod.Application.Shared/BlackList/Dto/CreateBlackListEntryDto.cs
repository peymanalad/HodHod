using System.ComponentModel.DataAnnotations;

namespace HodHod.BlackLists.Dto;

public class CreateBlackListEntryDto
{
    [Required]
    public string PhoneNumber { get; set; }
    public bool IsTransferred { get; set; }
}