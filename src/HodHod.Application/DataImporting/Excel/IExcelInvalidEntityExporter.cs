using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Dependency;
using HodHod.Dto;

namespace HodHod.DataImporting.Excel;

public interface IExcelInvalidEntityExporter<TEntityDto> : ITransientDependency
{
    Task<FileDto> ExportToFile(List<TEntityDto> entities);
}