using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace HodHod.Web.Startup;

public class Program
{
    public static void Main(string[] args)
    {
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

