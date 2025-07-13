using Abp.AutoMapper;
using HodHod.Sessions.Dto;

namespace HodHod.Maui.Models.Common;

[AutoMapFrom(typeof(ApplicationInfoDto)),
 AutoMapTo(typeof(ApplicationInfoDto))]
public class ApplicationInfoPersistanceModel
{
    public string Version { get; set; }

    public DateTime ReleaseDate { get; set; }

    public bool IsQrLoginEnabled { get; set; }
}