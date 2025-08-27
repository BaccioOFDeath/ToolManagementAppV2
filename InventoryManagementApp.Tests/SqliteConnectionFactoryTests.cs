using InventoryManagementApp.Data;
using Xunit;

public class SqliteConnectionFactoryTests
{
    [Fact]
    public void Create_ExecutesPragmasEachTime()
    {
        SqliteConnectionFactory.Reset();
        var factory = new SqliteConnectionFactory("Data Source=:memory:");
        using var first = factory.Create();
        using var second = factory.Create();
        Assert.Equal(2, SqliteConnectionFactory.PragmasExecutionCount);
    }
}
