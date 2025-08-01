﻿using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace HodHod.Web.Public.Startup;

public class Program
{
    public static void Main(string[] args)
    {
        DotNetEnv.Env.TraversePath().Load();
        CreateWebHostBuilder(args).Build().Run();
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args)
    {
        return new WebHostBuilder()
            .UseKestrel(opt => opt.AddServerHeader = false)
            .UseContentRoot(Directory.GetCurrentDirectory())
            .UseIIS()
            .UseIISIntegration()
            .UseStartup<Startup>();
    }
}

