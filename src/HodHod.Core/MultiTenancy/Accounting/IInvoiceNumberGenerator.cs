using System.Threading.Tasks;
using Abp.Dependency;

namespace HodHod.MultiTenancy.Accounting;

public interface IInvoiceNumberGenerator : ITransientDependency
{
    Task<string> GetNewInvoiceNumber();
}

