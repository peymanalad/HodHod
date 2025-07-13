﻿using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using HodHod.Authorization;
using HodHod.EntityChanges;
using HodHod.EntityChanges.Dto;
using HodHod.Web.Areas.App.Models.EntityChanges;
using HodHod.Web.Controllers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HodHod.Web.Areas.App.Controllers;

[Area("App")]
[AbpMvcAuthorize(AppPermissions.Pages_Administration_EntityChanges_FullHistory)]
public class EntityChangesController : HodHodControllerBase
{
    private readonly IEntityChangeAppService _entityChangeAppService;

    public EntityChangesController(IEntityChangeAppService entityChangeAppService)
    {
        _entityChangeAppService = entityChangeAppService;
    }

    [HttpGet]
    [Route("/App/EntityChanges/{entityId}/{entityTypeFullName}")]
    public async Task<IActionResult> Index(string entityId, string entityTypeFullName)
    {
        var entityChanges = await _entityChangeAppService.GetEntityChangesByEntity(new GetEntityChangesByEntityInput
        {
            EntityId = entityId,
            EntityTypeFullName = entityTypeFullName,
        });

        ViewBag.ChangesCount = entityChanges.Items.Count;
        ViewBag.EntityTypeShortName = entityTypeFullName.Substring(entityTypeFullName.LastIndexOf('.') + 1);
        ViewBag.EntityId = entityId;

        var viewModel = new EntityChangeListViewModel
        {
            EntityAndPropertyChanges = ObjectMapper.Map<List<EntityAndPropertyChangeListDto>>(entityChanges.Items)
        };

        return View(viewModel);
    }
}

