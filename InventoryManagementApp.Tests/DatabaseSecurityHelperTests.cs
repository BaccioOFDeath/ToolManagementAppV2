using InventoryManagementApp.Utilities.Helpers;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class DatabaseSecurityHelperTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("non-existent-file.db")]
        public void GetPermissionWarning_WhenFileMissing_ReturnsNull(string path)
        {
            var warning = DatabaseSecurityHelper.GetPermissionWarning(path);

            Assert.Null(warning);
        }
    }
}
