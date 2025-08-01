﻿using System.Collections.Generic;
using HodHod.Editions.Dto;
using HodHod.Security;

namespace HodHod.Web.Areas.App.Models.Tenants;

public class CreateTenantViewModel
{
    public IReadOnlyList<SubscribableEditionComboboxItemDto> EditionItems { get; set; }

    public PasswordComplexitySetting PasswordComplexitySetting { get; set; }

    public CreateTenantViewModel(IReadOnlyList<SubscribableEditionComboboxItemDto> editionItems)
    {
        EditionItems = editionItems;
    }
}

