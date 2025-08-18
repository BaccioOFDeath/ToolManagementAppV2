using ToolManagementAppV2.Utilities.Helpers;
using Xunit;

public class PasswordValidatorTests
{
    [Theory]
    [InlineData("short", false)]
    [InlineData("NoDigits!", false)]
    [InlineData("noupper1!", false)]
    [InlineData("NOLOWER1!", false)]
    [InlineData("Valid1!", true)]
    public void IsValid_ReturnsExpected(string pwd, bool expected)
    {
        var result = PasswordValidator.IsValid(pwd, out _);
        Assert.Equal(expected, result);
    }
}
