using System.Linq;

namespace ToolManagementAppV2.Utilities.Helpers
{
    public static class PasswordValidator
    {
        public static bool IsValid(string password, out string? error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            {
                error = "Password must be at least 8 characters long.";
                return false;
            }

            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));
            if (!(hasUpper && hasLower && hasDigit && hasSpecial))
            {
                error = "Password must contain upper, lower, digit, and special characters.";
                return false;
            }

            return true;
        }
    }
}
