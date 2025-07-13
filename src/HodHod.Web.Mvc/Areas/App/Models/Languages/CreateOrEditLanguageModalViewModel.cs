using Abp.AutoMapper;
using HodHod.Localization.Dto;

namespace HodHod.Web.Areas.App.Models.Languages;

[AutoMapFrom(typeof(GetLanguageForEditOutput))]
public class CreateOrEditLanguageModalViewModel : GetLanguageForEditOutput
{
    public bool IsEditMode => Language.Id.HasValue;
}

