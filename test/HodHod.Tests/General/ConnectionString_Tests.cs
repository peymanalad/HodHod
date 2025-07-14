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
        var csb = new SqlConnectionStringBuilder("Server=mssql; Database=HodHodDb; User=sa; Password=Ch@mr@nCh@mr@n; TrustServerCertificate=True");
        csb["Database"].ShouldBe("HodHodDb");
    }
}
