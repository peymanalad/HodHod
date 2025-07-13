using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using HodHod.MultiTenancy.Accounting.Dto;

namespace HodHod.MultiTenancy.Accounting;

public interface IInvoiceAppService
{
    Task<InvoiceDto> GetInvoiceInfo(EntityDto<long> input);

    Task CreateInvoice(CreateInvoiceDto input);
}
