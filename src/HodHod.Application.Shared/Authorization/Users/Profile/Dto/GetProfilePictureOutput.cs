﻿namespace HodHod.Authorization.Users.Profile.Dto;

public class GetProfilePictureOutput
{
    public string ProfilePicture { get; set; }

    public GetProfilePictureOutput(string profilePicture)
    {
        ProfilePicture = profilePicture;
    }
}

