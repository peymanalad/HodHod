﻿using System.ComponentModel.DataAnnotations;

namespace HodHod.Editions.Dto;

public class EditionCreateDto
{
    public int? Id { get; set; }

    [Required]
    public string DisplayName { get; set; }

    public decimal? MonthlyPrice { get; set; }

    public decimal? AnnualPrice { get; set; }

    public int? TrialDayCount { get; set; }

    public int? WaitingDayAfterExpire { get; set; }

    public int? ExpiringEditionId { get; set; }
}

