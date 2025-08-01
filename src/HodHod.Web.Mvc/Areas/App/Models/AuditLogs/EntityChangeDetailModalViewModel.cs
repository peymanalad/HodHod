﻿using System;
using System.Collections.Generic;
using HodHod.EntityChanges.Dto;

namespace HodHod.Web.Areas.App.Models.AuditLogs;

public class EntityChangeDetailModalViewModel
{
    public string EntityTypeFullName { get; set; }

    public DateTime ChangeTime { get; set; }

    public string UserName { get; set; }

    public string ImpersonatorUserName { get; set; }

    public List<EntityPropertyChangeDto> EntityPropertyChanges { get; set; }

    public EntityChangeDetailModalViewModel(List<EntityPropertyChangeDto> output, EntityChangeListDto entityChangeListDto)
    {
        EntityPropertyChanges = output;
        EntityTypeFullName = entityChangeListDto.EntityTypeFullName;
        ChangeTime = entityChangeListDto.ChangeTime;
        UserName = entityChangeListDto.UserName;
        ImpersonatorUserName = entityChangeListDto.ImpersonatorUserName;
    }
}

