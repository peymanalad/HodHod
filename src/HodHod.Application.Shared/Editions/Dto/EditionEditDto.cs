using System.ComponentModel.DataAnnotations;

namespace HodHod.Editions.Dto;

public class EditionEditDto
{
    public int? Id { get; set; }

    [Required]
    public string DisplayName { get; set; }

    public int? ExpiringEditionId { get; set; }
}

