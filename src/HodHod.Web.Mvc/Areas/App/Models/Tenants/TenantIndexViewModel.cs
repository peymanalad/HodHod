using System.Collections.Generic;
using HodHod.Editions.Dto;

namespace HodHod.Web.Areas.App.Models.Tenants;

public class TenantIndexViewModel
{
    public List<SubscribableEditionComboboxItemDto> EditionItems { get; set; }
}

