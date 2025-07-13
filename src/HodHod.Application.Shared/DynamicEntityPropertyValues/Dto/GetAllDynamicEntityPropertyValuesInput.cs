using System.ComponentModel.DataAnnotations;

namespace HodHod.DynamicEntityPropertyValues.Dto;

public class GetAllDynamicEntityPropertyValuesInput
{
    [Required]
    public string EntityFullName { get; set; }

    [Required]
    public string EntityId { get; set; }
}

