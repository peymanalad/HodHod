﻿using Abp.Application.Services.Dto;

namespace HodHod.Sessions.Dto;

public class UserLoginInfoDto : EntityDto<long>
{
    public string Name { get; set; }

    public string Surname { get; set; }

    public string UserName { get; set; }

    public string EmailAddress { get; set; }

    public string ProfilePictureId { get; set; }

    public LoginType LoginType { get; set; }
}

