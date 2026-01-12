using System;
using System.Linq;

namespace InventoryManagementApp.Utilities.Helpers
{
    public static class PasswordValidator
    {
        private const int MinimumLength = 8;
        private const int MaximumLength = 128;
        
        public static bool IsValid(string password, out string? error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(password))
            {
                error = "Password cannot be empty.";
                return false;
            }

            if (password.Length < MinimumLength)
            {
                error = $"Password must be at least {MinimumLength} characters long.";
                return false;
            }

            if (password.Length > MaximumLength)
            {
                error = $"Password must not exceed {MaximumLength} characters.";
                return false;
            }

            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);

            if (!hasUpper || !hasLower || !hasDigit)
            {
                error = "Password must contain at least one uppercase letter, one lowercase letter, and one digit.";
                return false;
            }

            return true;
        }
    }
}
