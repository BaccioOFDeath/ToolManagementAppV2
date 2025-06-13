using System.Globalization;
using System.Windows.Data;
using ToolManagementAppV2.Utilities.Converters;
using Xunit;

namespace ToolManagementAppV2.Tests
{
    public class NonEmptyStringToBoolConverterTests
    {
        [Fact]
        public void ConvertBack_True_ReturnsEmptyString()
        {
            var converter = new NonEmptyStringToBoolConverter();
            var result = converter.ConvertBack(true, typeof(string), null, CultureInfo.InvariantCulture);
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void ConvertBack_False_ReturnsNull()
        {
            var converter = new NonEmptyStringToBoolConverter();
            var result = converter.ConvertBack(false, typeof(string), null, CultureInfo.InvariantCulture);
            Assert.Null(result);
        }

        [Fact]
        public void ConvertBack_InvalidInput_ReturnsBindingDoNothing()
        {
            var converter = new NonEmptyStringToBoolConverter();
            var result = converter.ConvertBack(42, typeof(string), null, CultureInfo.InvariantCulture);
            Assert.Equal(Binding.DoNothing, result);
        }
    }
}
