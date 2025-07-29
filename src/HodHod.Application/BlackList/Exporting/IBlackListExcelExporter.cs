using System.Collections.Generic;
using System.Threading.Tasks;
using HodHod.BlackLists.Dto;
using HodHod.Dto;

namespace HodHod.BlackLists.Exporting;

public interface IBlackListExcelExporter
{
    Task<FileDto> ExportToFile(List<BlackListEntryDto> entries);
}