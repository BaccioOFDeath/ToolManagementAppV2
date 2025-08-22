using System;
using System.Globalization;
using System.Windows.Data;
using InventoryManagementApp.Utilities.Converters;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class DateTimeMinToEmptyStringConverterTests
    {
        [Fact]
        public void Convert_MinValue_ReturnsNull()
        {
            var converter = new DateTimeMinToEmptyStringConverter();
            var result = converter.Convert(DateTime.MinValue, typeof(DateTime), null, CultureInfo.InvariantCulture);
            Assert.Null(result);
        }

        [Fact]
        public void Convert_ValidDate_ReturnsSameDate()
        {
            var converter = new DateTimeMinToEmptyStringConverter();
            var date = new DateTime(2024, 5, 1, 12, 0, 0);
            var result = converter.Convert(date, typeof(DateTime), null, CultureInfo.InvariantCulture);
            Assert.Equal(date, result);
        }

        [Fact]
        public void Convert_InvalidInput_ReturnsBindingDoNothing()
        {
            var converter = new DateTimeMinToEmptyStringConverter();
            var result = converter.Convert("not a date", typeof(DateTime), null, CultureInfo.InvariantCulture);
            Assert.Equal(Binding.DoNothing, result);
        }

        [Fact]
        public void ConvertBack_EmptyString_ReturnsMinValue()
        {
            var converter = new DateTimeMinToEmptyStringConverter();
            var result = converter.ConvertBack(string.Empty, typeof(DateTime), null, CultureInfo.InvariantCulture);
            Assert.Equal(DateTime.MinValue, result);
        }

        [Fact]
        public void ConvertBack_ValidDateString_ReturnsDate()
        {
            var converter = new DateTimeMinToEmptyStringConverter();
            var date = new DateTime(2023, 12, 25);
            var result = converter.ConvertBack(date.ToString(CultureInfo.InvariantCulture), typeof(DateTime), null, CultureInfo.InvariantCulture);
            Assert.Equal(date, result);
        }

        [Fact]
        public void ConvertBack_InvalidInput_ReturnsBindingDoNothing()
        {
            var converter = new DateTimeMinToEmptyStringConverter();
            var result = converter.ConvertBack(42, typeof(DateTime), null, CultureInfo.InvariantCulture);
            Assert.Equal(Binding.DoNothing, result);
        }
    }
}
