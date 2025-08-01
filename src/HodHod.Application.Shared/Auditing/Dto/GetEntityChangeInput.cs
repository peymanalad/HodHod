﻿using System;
using Abp.Extensions;
using Abp.Runtime.Validation;
using HodHod.Common;
using HodHod.Dto;

namespace HodHod.Auditing.Dto;

public class GetEntityChangeInput : PagedAndSortedInputDto, IShouldNormalize
{
    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string UserName { get; set; }

    public string EntityTypeFullName { get; set; }

    public void Normalize()
    {
        if (Sorting.IsNullOrWhiteSpace())
        {
            Sorting = "ChangeTime DESC";
        }

        Sorting = DtoSortingHelper.ReplaceSorting(Sorting, s =>
        {

            if (s.IndexOf("UserName", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "User." + s;
            }

            return "EntityChange." + s;
        });
    }
}

public class GetEntityTypeChangeInput : PagedAndSortedInputDto, IShouldNormalize
{
    public string EntityTypeFullName { get; set; }

    public string EntityId { get; set; }

    public void Normalize()
    {
        if (Sorting.IsNullOrWhiteSpace())
        {
            Sorting = "ChangeTime DESC";
        }

        Sorting = DtoSortingHelper.ReplaceSorting(Sorting, s =>
        {
            if (s.IndexOf("UserName", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                s = "User." + s;
            }
            else
            {
                s = "EntityChange." + s;
            }

            return s;
        });
    }
}

