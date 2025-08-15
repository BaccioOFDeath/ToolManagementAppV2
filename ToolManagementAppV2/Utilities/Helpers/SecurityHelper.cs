using System;
using System.Security.Cryptography;
using System.Text;
using ToolManagementAppV2.Interfaces;

namespace ToolManagementAppV2.Utilities.Helpers
{
    public static class SecurityHelper
    {
        const int DefaultIterations = 100_000;
        static Lazy<int> _iterations = CreateIterations();
        static ISettingsService? _settingsService;

        public static ISettingsService? SettingsService
        {
            get => _settingsService;
            set
            {
                _settingsService = value;
                _iterations = CreateIterations();
            }
        }
        const string PasswordChars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@#$%^&*";

        public static bool IsSha256Hash(string input)
        {
            if (string.IsNullOrWhiteSpace(input) || input.Length != 64)
                return false;

            foreach (var c in input)
            {
                if (!Uri.IsHexDigit(c))
                    return false;
            }
            return true;
        }

        public static string HashPassword(string password, out string salt)
        {
            var saltBytes = new byte[16];
            RandomNumberGenerator.Fill(saltBytes);
            salt = Convert.ToBase64String(saltBytes);
            return HashPassword(password, salt);
        }

        public static string HashPassword(string password, string salt)
        {
            var saltBytes = Convert.FromBase64String(salt);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, GetIterations(), HashAlgorithmName.SHA256);
            return Convert.ToBase64String(pbkdf2.GetBytes(32));
        }

        public static bool VerifyPassword(string password, string salt, string hash)
        {
            if (string.IsNullOrEmpty(salt) || string.IsNullOrEmpty(hash))
                return false;

            var computed = HashPassword(password, salt);
            try
            {
                var computedBytes = Convert.FromBase64String(computed);
                var hashBytes = Convert.FromBase64String(hash);
                return CryptographicOperations.FixedTimeEquals(computedBytes, hashBytes);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public static string GeneratePassword(int length = 12)
        {
            var bytes = new byte[length];
            RandomNumberGenerator.Fill(bytes);
            var chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = PasswordChars[bytes[i] % PasswordChars.Length];
            }
            return new string(chars);
        }

        // Legacy support for migrating existing SHA256 hashes
        public static string ComputeSha256HashLegacy(string rawData)
        {
            using var sha256Hash = SHA256.Create();
            var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            var builder = new StringBuilder();
            foreach (var b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }

        static Lazy<int> CreateIterations() => new(() =>
        {
            var value = _settingsService?.GetPasswordIterations();
            return value > 0 ? value : DefaultIterations;
        }, true);

        static int GetIterations() => _iterations.Value;
    }
}
