using Abp.Application.Services.Dto;
using System;

namespace HodHod.Categories.Dto;

public class SubCategoryDto
{
    public Guid PublicId { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; }
}