using Abp.Dependency;

namespace HodHod;

public class AppFolders : IAppFolders, ISingletonDependency
{
    public string SampleProfileImagesFolder { get; set; }

    public string WebLogsFolder { get; set; }
    public string ReportFilesFolder { get; set; }
}

