using HodHod.MultiTenancy.Dto;
using HodHod.Sessions.Dto;

namespace HodHod.Web.Areas.App.Models.Editions;

public class SubscriptionDashboardViewModel
{
    public GetCurrentLoginInformationsOutput LoginInformations { get; set; }

    public EditionsSelectOutput Editions { get; set; }
}

