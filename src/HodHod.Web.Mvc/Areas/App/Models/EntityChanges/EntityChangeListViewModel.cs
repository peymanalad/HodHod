using Abp.AutoMapper;
using HodHod.EntityChanges.Dto;
using System.Collections.Generic;

namespace HodHod.Web.Areas.App.Models.EntityChanges;

[AutoMapFrom(typeof(EntityAndPropertyChangeListDto))]
public class EntityChangeListViewModel
{
    public List<EntityAndPropertyChangeListDto> EntityAndPropertyChanges { get; set; }
}

