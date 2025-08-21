using System;

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

            return true;
        }
    }
}
