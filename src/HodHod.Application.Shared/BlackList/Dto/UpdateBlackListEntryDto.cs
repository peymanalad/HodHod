using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;

namespace HodHod.BlackLists.Dto;

public class UpdateBlackListEntryDto : EntityDto<int>
{
    [Required]
    public string PhoneNumber { get; set; }
    public bool IsTransferred { get; set; }
}