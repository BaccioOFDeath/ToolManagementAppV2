using Xunit;
using InventoryManagementApp.Utilities.Helpers;

namespace InventoryManagementApp.Tests
{
    public class PasswordValidatorTests
    {
        [Fact]
        public void IsValid_EmptyPassword_ReturnsFalse()
        {
            // Arrange
            string password = "";

            // Act
            var result = PasswordValidator.IsValid(password, out var error);

            // Assert
            Assert.False(result);
            Assert.Equal("Password cannot be empty.", error);
        }

        [Fact]
        public void IsValid_NullPassword_ReturnsFalse()
        {
            // Arrange
            string? password = null;

            // Act
            var result = PasswordValidator.IsValid(password!, out var error);

            // Assert
            Assert.False(result);
            Assert.Equal("Password cannot be empty.", error);
        }

        [Fact]
        public void IsValid_WhitespacePassword_ReturnsFalse()
        {
            // Arrange
            string password = "   ";

            // Act
            var result = PasswordValidator.IsValid(password, out var error);

            // Assert
            Assert.False(result);
            Assert.Equal("Password cannot be empty.", error);
        }

        [Fact]
        public void IsValid_SingleDigitPassword_ReturnsTrue()
        {
            string password = "1";

            var result = PasswordValidator.IsValid(password, out var error);

            Assert.True(result);
            Assert.Null(error);
        }

        [Fact]
        public void IsValid_ShortNumericPassword_ReturnsTrue()
        {
            string password = "123";

            var result = PasswordValidator.IsValid(password, out var error);

            Assert.True(result);
            Assert.Null(error);
        }

        [Fact]
        public void IsValid_ShortAlphabeticPassword_ReturnsTrue()
        {
            string password = "bmw";

            var result = PasswordValidator.IsValid(password, out var error);

            Assert.True(result);
            Assert.Null(error);
        }

        [Fact]
        public void IsValid_LongPassword_ReturnsTrue()
        {
            string password = "ThisIsAVeryLongPassword123WithLotsOfCharacters";

            var result = PasswordValidator.IsValid(password, out var error);

            Assert.True(result);
            Assert.Null(error);
        }
    }
}
