namespace HodHod.Geo.Dto;

using System.Collections.Generic;

public class ProvinceDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<CityDto> Cities { get; set; }
}