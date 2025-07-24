using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using HodHod.EntityFrameworkCore;
using HodHod.Geo;
using System;
using System.Text.Json.Serialization;

namespace HodHod.Migrations.Seed.Host;

public class DefaultLocationCreator
{
    private readonly HodHodDbContext _context;

    public DefaultLocationCreator(HodHodDbContext context)
    {
        _context = context;
    }

    public void Create()
    {
        if (_context.Provinces.Any())
        {
            return;
        }

        var filePath = Path.Combine(AppContext.BaseDirectory, "provinces.json");
        if (!File.Exists(filePath))
        {
            return;
        }

        var json = File.ReadAllText(filePath);
        var records = JsonSerializer.Deserialize<List<ProvinceRecord>>(json);
        if (records == null)
        {
            return;
        }

        foreach (var r in records)
        {
            var province = new Province
            {
                Name = r.Province,
                Cities = new List<City>()
            };
            foreach (var city in r.Cities ?? Enumerable.Empty<string>())
            {
                province.Cities.Add(new City { Name = city });
            }
            _context.Provinces.Add(province);
        }

        _context.SaveChanges();
    }

    private class ProvinceRecord
    {
        [JsonPropertyName("province")]
        public string Province { get; set; }
        [JsonPropertyName("cities")]
        public List<string> Cities { get; set; } = new();
    }
}