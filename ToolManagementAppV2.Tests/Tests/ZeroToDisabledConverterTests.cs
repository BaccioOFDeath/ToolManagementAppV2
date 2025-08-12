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
        public void ConvertBack_Bool_ReturnsBindingDoNothing()
        {
            var c = new ZeroToDisabledConverter();
            var result = c.ConvertBack(true, typeof(int), null, CultureInfo.InvariantCulture);
            Assert.Equal(System.Windows.Data.Binding.DoNothing, result);
        }

        [Fact]
        public void ConvertBack_Int_ReturnsSameValue()
        {
            var c = new ZeroToDisabledConverter();
            var result = c.ConvertBack(3, typeof(int), null, CultureInfo.InvariantCulture);
            Assert.Equal(3, result);
        }
    }
}
