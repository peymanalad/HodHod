using HodHod.Dto;
using System;

namespace HodHod.EntityChanges.Dto;

public class GetEntityChangesByEntityInput
{
    public string EntityTypeFullName { get; set; }
    public string EntityId { get; set; }
}

