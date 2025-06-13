using System.Globalization;
using System.Windows.Data;
using ToolManagementAppV2.Utilities.Converters;
using Xunit;

namespace ToolManagementAppV2.Tests
{
    public class CheckOutStatusConverterTests
    {
        [Fact]
        public void ConvertBack_CheckIn_ReturnsTrue()
        {
            var converter = new CheckOutStatusConverter();
            var result = converter.ConvertBack("Check In", typeof(bool), null, CultureInfo.InvariantCulture);
            Assert.True((bool)result);
        }

        [Fact]
        public void ConvertBack_CheckOut_ReturnsFalse()
        {
            var converter = new CheckOutStatusConverter();
            var result = converter.ConvertBack("Check Out", typeof(bool), null, CultureInfo.InvariantCulture);
            Assert.False((bool)result);
        }

        [Fact]
        public void ConvertBack_InvalidInput_ReturnsBindingDoNothing()
        {
            var converter = new CheckOutStatusConverter();
            var result = converter.ConvertBack(42, typeof(bool), null, CultureInfo.InvariantCulture);
            Assert.Equal(Binding.DoNothing, result);
        }
    }
}
