using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using ToolManagementAppV2.Utilities.Converters;
using Xunit;

namespace ToolManagementAppV2.Tests
{
    public class NullToDefaultImageConverterTests
    {
        [Fact]
        public void ConvertBack_BitmapImage_ReturnsUriString()
        {
            var converter = new NullToDefaultImageConverter();
            var uri = "pack://application:,,,/Resources/DefaultUserPhoto.png";
            var bmp = new BitmapImage(new Uri(uri));
            var result = converter.ConvertBack(bmp, typeof(string), null, CultureInfo.InvariantCulture);
            Assert.Equal(uri, result);
        }

        [Fact]
        public void ConvertBack_String_ReturnsSameString()
        {
            var converter = new NullToDefaultImageConverter();
            var path = "image.png";
            var result = converter.ConvertBack(path, typeof(string), null, CultureInfo.InvariantCulture);
            Assert.Equal(path, result);
        }

        [Fact]
        public void ConvertBack_InvalidInput_ReturnsBindingDoNothing()
        {
            var converter = new NullToDefaultImageConverter();
            var result = converter.ConvertBack(42, typeof(string), null, CultureInfo.InvariantCulture);
            Assert.Equal(Binding.DoNothing, result);
        }
    }
}
