﻿using Abp.Application.Services.Dto;

namespace HodHod.Dto;

public class PagedAndSortedInputDto : PagedInputDto, ISortedResultRequest
{
    public string Sorting { get; set; }

    public PagedAndSortedInputDto()
    {
        MaxResultCount = AppConsts.DefaultPageSize;
    }
}

