using Abp.AutoMapper;
using HodHod.Authorization.Users.Dto;

namespace HodHod.Maui.Models.User;

[AutoMapFrom(typeof(UserListDto))]
public class UserListModel : UserListDto
{
    public string Photo { get; set; }

    public string FullName => Name + " " + Surname;
}