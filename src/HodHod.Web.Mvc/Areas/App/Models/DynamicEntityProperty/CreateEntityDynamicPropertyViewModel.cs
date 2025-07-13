using System.Collections.Generic;
using HodHod.DynamicEntityProperties.Dto;

namespace HodHod.Web.Areas.App.Models.DynamicEntityProperty;

public class CreateEntityDynamicPropertyViewModel
{
    public string EntityFullName { get; set; }

    public List<string> AllEntities { get; set; }

    public List<DynamicPropertyDto> DynamicProperties { get; set; }
}

