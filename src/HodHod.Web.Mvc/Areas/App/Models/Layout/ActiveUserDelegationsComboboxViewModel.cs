using System.Collections.Generic;
using HodHod.Authorization.Delegation;
using HodHod.Authorization.Users.Delegation.Dto;

namespace HodHod.Web.Areas.App.Models.Layout;

public class ActiveUserDelegationsComboboxViewModel
{
    public IUserDelegationConfiguration UserDelegationConfiguration { get; set; }

    public List<UserDelegationDto> UserDelegations { get; set; }

    public string CssClass { get; set; }
}

