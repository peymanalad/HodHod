using System.Collections.Generic;
using HodHod.Editions.Dto;
using HodHod.MultiTenancy.Dto;

namespace HodHod.Web.Areas.App.Models.Tenants;

public class EditTenantViewModel
{
    public TenantEditDto Tenant { get; set; }

    public IReadOnlyList<SubscribableEditionComboboxItemDto> EditionItems { get; set; }

    public EditTenantViewModel(TenantEditDto tenant, IReadOnlyList<SubscribableEditionComboboxItemDto> editionItems)
    {
        Tenant = tenant;
        EditionItems = editionItems;
    }
}

