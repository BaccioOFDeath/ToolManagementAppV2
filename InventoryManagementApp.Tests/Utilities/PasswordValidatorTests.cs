using InventoryManagementApp.Utilities.Helpers;
using Xunit;

public class PasswordValidatorTests
{
    [Theory]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("pass", true)]
    [InlineData("123456", true)]
    public void IsValid_ReturnsExpected(string pwd, bool expected)
    {
        var result = PasswordValidator.IsValid(pwd, out _);
        Assert.Equal(expected, result);
    }
}
