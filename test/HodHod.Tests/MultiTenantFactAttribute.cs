using Xunit;

namespace HodHod.Tests;

public sealed class MultiTenantFactAttribute : FactAttribute
{
    private readonly bool _multiTenancyEnabled = HodHodConsts.MultiTenancyEnabled;

    public MultiTenantFactAttribute()
    {
        if (!_multiTenancyEnabled)
        {
            Skip = "MultiTenancy is disabled.";
        }
    }
}
