using Microsoft.Data.SqlClient;
using Shouldly;
using Xunit;

namespace HodHod.Tests.General;

// ReSharper disable once InconsistentNaming
public class ConnectionString_Tests
{
    [Fact]
    public void SqlConnectionStringBuilder_Test()
    {
        var csb = new SqlConnectionStringBuilder("Server=localhost; Database=HodHod; Trusted_Connection=True;");
        csb["Database"].ShouldBe("HodHod");
    }
}
