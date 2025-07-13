using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Dependency;

namespace HodHod.DataImporting.Excel;

public interface IExcelDataReader<TEntityDto> : ITransientDependency
{
    Task<List<TEntityDto>> GetEntitiesFromExcel(byte[] fileBytes);
}