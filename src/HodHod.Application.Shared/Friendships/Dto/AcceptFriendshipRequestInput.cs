﻿using System.ComponentModel.DataAnnotations;

namespace HodHod.Friendships.Dto;

public class AcceptFriendshipRequestInput
{
    [Range(1, long.MaxValue)]
    public long UserId { get; set; }

    public int? TenantId { get; set; }
}

