using System.ComponentModel.DataAnnotations;

namespace HodHod.TokenAuth.Dto;

public class ImpersonateModel
{
    public int? TenantId { get; set; }

    [Range(1, long.MaxValue)]
    public long UserId { get; set; }
}