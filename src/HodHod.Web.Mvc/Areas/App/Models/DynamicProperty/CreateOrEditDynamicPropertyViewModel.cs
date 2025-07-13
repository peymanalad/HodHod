using System.Collections.Generic;
using HodHod.DynamicEntityProperties.Dto;

namespace HodHod.Web.Areas.App.Models.DynamicProperty;

public class CreateOrEditDynamicPropertyViewModel
{
    public DynamicPropertyDto DynamicPropertyDto { get; set; }

    public List<string> AllowedInputTypes { get; set; }
}

