using System.Collections.Generic;
using System.Threading.Tasks;
using Abp;
using HodHod.Chat.Dto;
using HodHod.Dto;

namespace HodHod.Chat.Exporting;

public interface IChatMessageListExcelExporter
{
    Task<FileDto> ExportToFile(UserIdentifier user, List<ChatMessageExportDto> messages);
}