using System.Collections.Generic;
using System.Threading.Tasks;
using HodHod.BlackLists.Dto;
using HodHod.DataExporting.Excel.MiniExcel;
using HodHod.Dto;
using HodHod.Storage;

namespace HodHod.BlackLists.Exporting;

public class BlackListExcelExporter(ITempFileCacheManager tempFileCacheManager)
    : MiniExcelExcelExporterBase(tempFileCacheManager), IBlackListExcelExporter
{
    public async Task<FileDto> ExportToFile(List<BlackListEntryDto> entries)
    {
        var items = new List<Dictionary<string, object>>();
        foreach (var entry in entries)
        {
            items.Add(new Dictionary<string, object>
            {
                {L("PhoneNumber"), entry.PhoneNumber},
                {"IsTransferred", entry.IsTransferred}
            });
        }
        return await CreateExcelPackageAsync("BlackList.xlsx", items);
    }
}