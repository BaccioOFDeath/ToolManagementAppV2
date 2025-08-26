using System.Globalization;
using InventoryManagementApp.Utilities.Converters;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class NameToInitialsConverterTests
    {
        [Theory]
        [InlineData("John Doe", "JD")]
        [InlineData("alice", "A")]
        [InlineData(" Bob Charles David ", "BD")]
        [InlineData("", "")]
        public void Convert_ReturnsExpectedInitials(string? name, string expected)
        {
            var converter = new NameToInitialsConverter();
            var result = converter.Convert(name, typeof(string), null, CultureInfo.InvariantCulture);
            Assert.Equal(expected, result);
        }
    }
}
