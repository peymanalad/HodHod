using Abp.Application.Services;
using HodHod.Dto;
using HodHod.Logging.Dto;

namespace HodHod.Logging;

public interface IWebLogAppService : IApplicationService
{
    GetLatestWebLogsOutput GetLatestWebLogs();

    FileDto DownloadWebLogs();
}

