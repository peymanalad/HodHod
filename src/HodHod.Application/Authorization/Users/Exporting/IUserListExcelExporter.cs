using System.Collections.Generic;
using System.Threading.Tasks;
using HodHod.Authorization.Users.Dto;
using HodHod.Dto;

namespace HodHod.Authorization.Users.Exporting;

public interface IUserListExcelExporter
{
    Task<FileDto> ExportToFile(List<UserListDto> userListDtos, List<string> selectedColumns);
}