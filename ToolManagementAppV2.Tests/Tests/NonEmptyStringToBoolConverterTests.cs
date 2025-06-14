using System.Globalization;
using System.Windows.Data;
using ToolManagementAppV2.Utilities.Converters;
using Xunit;

namespace ToolManagementAppV2.Tests
{
    public class NonEmptyStringToBoolConverterTests
    {
        [Fact]
        public void Convert_NonEmptyString_ReturnsTrue()
        {
            var converter = new NonEmptyStringToBoolConverter();
            var result = converter.Convert("text", typeof(bool), null, CultureInfo.InvariantCulture);
            Assert.True((bool)result);
        }

        [Fact]
        public void Convert_EmptyOrNull_ReturnsFalse()
        {
            var converter = new NonEmptyStringToBoolConverter();
            Assert.False((bool)converter.Convert(string.Empty, typeof(bool), null, CultureInfo.InvariantCulture));
            Assert.False((bool)converter.Convert(null, typeof(bool), null, CultureInfo.InvariantCulture));
        }

        [Fact]
        public void Convert_InvalidInput_ReturnsFalse()
        {
            var converter = new NonEmptyStringToBoolConverter();
            var result = converter.Convert(42, typeof(bool), null, CultureInfo.InvariantCulture);
            Assert.False((bool)result);
        }

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
