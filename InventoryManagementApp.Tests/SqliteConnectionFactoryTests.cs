using InventoryManagementApp.Data;
using Xunit;

public class SqliteConnectionFactoryTests
{
    [Fact]
    public void Create_ExecutesPragmasOnlyOnce()
    {
        SqliteConnectionFactory.Reset();
        var factory = new SqliteConnectionFactory("Data Source=:memory:");
        using var first = factory.Create();
        using var second = factory.Create();
        Assert.Equal(1, SqliteConnectionFactory.PragmasExecutionCount);
    }
}
