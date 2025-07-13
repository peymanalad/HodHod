using System.Collections.Generic;
using Abp.Application.Services.Dto;
using HodHod.Editions.Dto;

namespace HodHod.Web.Areas.App.Models.Common;

public interface IFeatureEditViewModel
{
    List<NameValueDto> FeatureValues { get; set; }

    List<FlatFeatureDto> Features { get; set; }
}

