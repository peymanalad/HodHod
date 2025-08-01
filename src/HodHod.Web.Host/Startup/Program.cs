﻿using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace HodHod.Web.Startup;

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
            .UseKestrel(opt =>
            {
                opt.AddServerHeader = false;
                opt.Limits.MaxRequestLineSize = 16 * 1024;
                opt.Limits.MaxRequestBodySize = 50 * 1024 * 1024; // 50MB

                opt.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
                opt.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(2);
                //opt.Limits.RequestBodyTimeout = TimeSpan.FromMinutes(2);
            })
            .UseContentRoot(Directory.GetCurrentDirectory())
            .ConfigureLogging((context, logging) =>
            {
                logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
            })
            .UseIIS()
            .UseIISIntegration()
            .UseStartup<Startup>();
    }
}

