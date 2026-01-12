using InventoryManagementApp.Utilities.Helpers;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class PasswordDefaultsTests
    {
        [Theory]
        [InlineData(PasswordDefaults.DefaultAdminPassword)]
        [InlineData(PasswordDefaults.TemporaryPassword)]
        public void IsDefaultPassword_MatchesKnownDefaults(string password)
        {
            Assert.True(PasswordDefaults.IsDefaultPassword(password));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("Password123")]
        public void IsDefaultPassword_IgnoresNonDefaults(string password)
        {
            Assert.False(PasswordDefaults.IsDefaultPassword(password));
        }
    }
}
