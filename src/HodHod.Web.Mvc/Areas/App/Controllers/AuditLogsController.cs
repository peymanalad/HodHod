using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.Auditing;
using Microsoft.AspNetCore.Mvc;
using HodHod.Auditing;
using HodHod.Authorization;
using HodHod.EntityChanges.Dto;
using HodHod.Web.Areas.App.Models.AuditLogs;
using HodHod.Web.Controllers;

namespace HodHod.Web.Areas.App.Controllers;

[Area("App")]
[DisableAuditing]
[AbpMvcAuthorize(AppPermissions.Pages_Administration_AuditLogs)]
public class AuditLogsController : HodHodControllerBase
{
    private readonly IAuditLogAppService _auditLogAppService;

    public AuditLogsController(IAuditLogAppService auditLogAppService)
    {
        _auditLogAppService = auditLogAppService;
    }

    public ActionResult Index()
    {
        return View();
    }

    public async Task<PartialViewResult> EntityChangeDetailModal(EntityChangeListDto entityChangeListDto)
    {
        var output = await _auditLogAppService.GetEntityPropertyChanges(entityChangeListDto.Id);

        var viewModel = new EntityChangeDetailModalViewModel(output, entityChangeListDto);

        return PartialView("_EntityChangeDetailModal", viewModel);
    }
}

