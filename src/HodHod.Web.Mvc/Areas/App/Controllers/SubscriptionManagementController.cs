﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using HodHod.Authorization;
using HodHod.Editions;
using HodHod.MultiTenancy;
using HodHod.MultiTenancy.Payments;
using HodHod.Web.Areas.App.Models.Editions;
using HodHod.Web.Areas.App.Models.SubscriptionManagement;
using HodHod.Web.Controllers;
using HodHod.Web.Session;

namespace HodHod.Web.Areas.App.Controllers;

[Area("App")]
[AbpMvcAuthorize(AppPermissions.Pages_Administration_Tenant_SubscriptionManagement)]
public class SubscriptionManagementController : HodHodControllerBase
{
    private readonly IPaymentAppService _paymentAppService;
    private readonly IEditionAppService _editionAppService;
    private readonly ITenantRegistrationAppService _tenantRegistrationAppService;
    private readonly IPerRequestSessionCache _sessionCache;


    public SubscriptionManagementController(IPaymentAppService paymentAppService,
        IEditionAppService editionAppService,
        ITenantRegistrationAppService tenantRegistrationAppService,
        IPerRequestSessionCache sessionCache)
    {
        _paymentAppService = paymentAppService;
        _editionAppService = editionAppService;
        _sessionCache = sessionCache;
        _tenantRegistrationAppService = tenantRegistrationAppService;
    }

    public async Task<ActionResult> Index()
    {
        var loginInfo = await _sessionCache.GetCurrentLoginInformationsAsync();
        var editions = await _tenantRegistrationAppService.GetEditionsForSelect();
        var model = new SubscriptionDashboardViewModel
        {
            LoginInformations = loginInfo,
            Editions = editions
        };

        return View(model);
    }

    public async Task<PartialViewResult> ShowDetailModal(int id)
    {
        var payment = await _paymentAppService.GetPaymentAsync(id);

        var viewModel = ObjectMapper.Map<List<ShowDetailModalViewModel>>(payment.SubscriptionPaymentProducts);
        return PartialView("_ShowDetailModal", viewModel);
    }
}

