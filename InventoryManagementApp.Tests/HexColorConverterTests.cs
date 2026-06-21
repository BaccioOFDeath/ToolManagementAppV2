using InventoryManagementApp.Utilities.Converters;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class HexColorConverterTests
    {
        [Fact]
        public void Convert_AcceptsBareAndAlphaHexValues()
        {
            var converter = new HexColorConverter();

            Assert.Equal(Color.FromRgb(0x12, 0x34, 0x56), converter.Convert("123456", typeof(Color), null, CultureInfo.InvariantCulture));
            Assert.Equal(Color.FromArgb(0x80, 0x12, 0x34, 0x56), converter.Convert("#80123456", typeof(Color), null, CultureInfo.InvariantCulture));
        }

        [Fact]
        public void ConvertBack_WritesThemeHexString()
        {
            var converter = new HexColorConverter();

            Assert.Equal("#123456", converter.ConvertBack(Color.FromRgb(0x12, 0x34, 0x56), typeof(string), null, CultureInfo.InvariantCulture));
            Assert.Equal("#80123456", converter.ConvertBack(Color.FromArgb(0x80, 0x12, 0x34, 0x56), typeof(string), null, CultureInfo.InvariantCulture));
            Assert.Same(Binding.DoNothing, converter.ConvertBack(null, typeof(string), null, CultureInfo.InvariantCulture));
        }
    }
}
