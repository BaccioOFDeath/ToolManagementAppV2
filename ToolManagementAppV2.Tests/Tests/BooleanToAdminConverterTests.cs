using System.Globalization;
using System.Windows.Data;
using ToolManagementAppV2.Utilities.Converters;
using Xunit;

namespace ToolManagementAppV2.Tests
{
    public class BooleanToAdminConverterTests
    {
        [Fact]
        public void Convert_True_ReturnsAdmin()
        {
            var converter = new BooleanToAdminConverter();
            var result = converter.Convert(true, typeof(string), null, CultureInfo.InvariantCulture);
            Assert.Equal("Admin", result);
        }

        [Fact]
        public void Convert_False_ReturnsEmptyString()
        {
            var converter = new BooleanToAdminConverter();
            var result = converter.Convert(false, typeof(string), null, CultureInfo.InvariantCulture);
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void Convert_InvalidInput_ReturnsBindingDoNothing()
        {
            var converter = new BooleanToAdminConverter();
            var result = converter.Convert(5, typeof(string), null, CultureInfo.InvariantCulture);
            Assert.Equal(Binding.DoNothing, result);
        }

        [Fact]
        public void ConvertBack_AdminString_ReturnsTrue()
        {
            var converter = new BooleanToAdminConverter();
            var result = converter.ConvertBack("Admin", typeof(bool), null, CultureInfo.InvariantCulture);
            Assert.True((bool)result);
        }

        [Fact]
        public void ConvertBack_True_ReturnsTrue()
        {
            var converter = new BooleanToAdminConverter();
            var result = converter.ConvertBack(true, typeof(bool), null, CultureInfo.InvariantCulture);
            Assert.True((bool)result);
        }

        [Fact]
        public void ConvertBack_OtherString_ReturnsFalse()
        {
            var converter = new BooleanToAdminConverter();
            var result = converter.ConvertBack("User", typeof(bool), null, CultureInfo.InvariantCulture);
            Assert.False((bool)result);
        }

        [Fact]
        public void ConvertBack_InvalidInput_ReturnsBindingDoNothing()
        {
            var converter = new BooleanToAdminConverter();
            var result = converter.ConvertBack(42, typeof(bool), null, CultureInfo.InvariantCulture);
            Assert.Equal(Binding.DoNothing, result);
        }
    }
}
