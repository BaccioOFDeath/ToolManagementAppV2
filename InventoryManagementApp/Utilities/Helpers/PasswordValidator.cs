using System;
using System.Linq;

namespace InventoryManagementApp.Utilities.Helpers
{
    public static class PasswordValidator
    {
        public static bool IsValid(string password, out string? error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(password))
            {
                error = "Password cannot be empty.";
                return false;
            }

            if (password.Length < 8)
            {
                error = "Password must be at least 8 characters long.";
                return false;
            }

            if (!password.Any(char.IsUpper))
            {
                error = "Password must contain at least one uppercase letter.";
                return false;
            }

            if (!password.Any(char.IsLower))
            {
                error = "Password must contain at least one lowercase letter.";
                return false;
            }

            if (!password.Any(char.IsDigit))
            {
                error = "Password must contain at least one digit.";
                return false;
            }

            return true;
        }
    }
}
