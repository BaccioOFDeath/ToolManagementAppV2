using System;
using System.Globalization;
using ToolManagementAppV2.Utilities.Converters;
using Xunit;

namespace ToolManagementAppV2.Tests
{
    public class ZeroToDisabledConverterTests
    {
        [Fact]
        public void Convert_Positive_ReturnsTrue()
        {
            var c = new ZeroToDisabledConverter();
            Assert.True((bool)c.Convert(5, typeof(bool), null, CultureInfo.InvariantCulture));
        }

        [Fact]
        public void Convert_ZeroOrNegative_ReturnsFalse()
        {
            var c = new ZeroToDisabledConverter();
            Assert.False((bool)c.Convert(0, typeof(bool), null, CultureInfo.InvariantCulture));
            Assert.False((bool)c.Convert(-1, typeof(bool), null, CultureInfo.InvariantCulture));
        }

        [Fact]
        public void Convert_InvalidInput_ReturnsFalse()
        {
            var c = new ZeroToDisabledConverter();
            Assert.False((bool)c.Convert("bad", typeof(bool), null, CultureInfo.InvariantCulture));
        }

        [Fact]
        public void ConvertBack_Throws()
        {
            var c = new ZeroToDisabledConverter();
            Assert.Throws<NotImplementedException>(() => c.ConvertBack(true, typeof(int), null, CultureInfo.InvariantCulture));
        }
    }
}
