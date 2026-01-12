using System;
using System.Linq;

namespace InventoryManagementApp.Utilities.Helpers
{
    public static class PasswordDefaults
    {
        public const string DefaultAdminPassword = "Admin123";
        public const string TemporaryPassword = "TempPass1";

        public static readonly string[] ExpiringPasswords =
        {
            DefaultAdminPassword,
            TemporaryPassword
        };

        public static bool IsDefaultPassword(string? password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            return ExpiringPasswords.Any(p => string.Equals(p, password, StringComparison.Ordinal));
        }
    }
}
