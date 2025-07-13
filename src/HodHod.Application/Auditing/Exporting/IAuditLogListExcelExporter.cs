using HodHod.Auditing.Dto;
using HodHod.Dto;
using HodHod.EntityChanges.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HodHod.Auditing.Exporting;

public interface IAuditLogListExcelExporter
{
    Task<FileDto> ExportToFile(List<AuditLogListDto> auditLogListDtos);

    Task<FileDto> ExportToFile(List<EntityChangeListDto> entityChangeListDtos);
}
