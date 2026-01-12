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
        public void IsValid_TooShortPassword_ReturnsFalse()
        {
            // Arrange
            string password = "Pass1";

            // Act
            var result = PasswordValidator.IsValid(password, out var error);

            // Assert
            Assert.False(result);
            Assert.Contains("at least 8 characters", error);
        }

        [Fact]
        public void IsValid_NoUppercase_ReturnsFalse()
        {
            // Arrange
            string password = "password123";

            // Act
            var result = PasswordValidator.IsValid(password, out var error);

            // Assert
            Assert.False(result);
            Assert.Contains("uppercase letter", error);
        }

        [Fact]
        public void IsValid_NoLowercase_ReturnsFalse()
        {
            // Arrange
            string password = "PASSWORD123";

            // Act
            var result = PasswordValidator.IsValid(password, out var error);

            // Assert
            Assert.False(result);
            Assert.Contains("lowercase letter", error);
        }

        [Fact]
        public void IsValid_NoDigit_ReturnsFalse()
        {
            // Arrange
            string password = "Password";

            // Act
            var result = PasswordValidator.IsValid(password, out var error);

            // Assert
            Assert.False(result);
            Assert.Contains("digit", error);
        }

        [Fact]
        public void IsValid_ValidPassword_ReturnsTrue()
        {
            // Arrange
            string password = "Password123";

            // Act
            var result = PasswordValidator.IsValid(password, out var error);

            // Assert
            Assert.True(result);
            Assert.Null(error);
        }

        [Fact]
        public void IsValid_MinimumLengthValidPassword_ReturnsTrue()
        {
            // Arrange
            string password = "Pass1234";

            // Act
            var result = PasswordValidator.IsValid(password, out var error);

            // Assert
            Assert.True(result);
            Assert.Null(error);
        }

        [Fact]
        public void IsValid_LongValidPassword_ReturnsTrue()
        {
            // Arrange
            string password = "ThisIsAVeryLongPassword123WithLotsOfCharacters";

            // Act
            var result = PasswordValidator.IsValid(password, out var error);

            // Assert
            Assert.True(result);
            Assert.Null(error);
        }
    }
}
