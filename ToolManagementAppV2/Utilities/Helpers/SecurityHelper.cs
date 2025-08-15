using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ToolManagementAppV2.Interfaces;

namespace ToolManagementAppV2.Utilities.Helpers
{
    public static class SecurityHelper
    {
        const int DefaultIterations = 100_000;
        static ISettingsService? _settingsService;
        static int _iterationCache;
        static readonly object _iterLock = new();

        public static ISettingsService? SettingsService
        {
            get => _settingsService;
            set
            {
                _settingsService = value;
                Volatile.Write(ref _iterationCache, 0);
            }
        }

        const string PasswordChars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@#$%^&*";

        public static bool IsSha256Hash(string input)
        {
            if (string.IsNullOrWhiteSpace(input) || input.Length != 64) return false;
            foreach (var c in input) if (!Uri.IsHexDigit(c)) return false;
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
            if (string.IsNullOrEmpty(salt) || string.IsNullOrEmpty(hash)) return false;
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
            for (int i = 0; i < length; i++) chars[i] = PasswordChars[bytes[i] % PasswordChars.Length];
            return new string(chars);
        }

        public static string ComputeSha256HashLegacy(string rawData)
        {
            using var sha256Hash = SHA256.Create();
            var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        static int GetIterations()
        {
            var cached = Volatile.Read(ref _iterationCache);
            if (cached > 0) return cached;

            lock (_iterLock)
            {
                cached = _iterationCache;
                if (cached > 0) return cached;

                int value = DefaultIterations;
                var svc = _settingsService;
                if (svc != null)
                {
                    value = svc.GetPasswordIterations();
                    if (value <= 0)
                    {
                        value = svc.GetPasswordIterationsAsync()
                            .ConfigureAwait(false)
                            .GetAwaiter()
                            .GetResult();
                    }
                }

                if (value <= 0)
                    value = DefaultIterations;

                Volatile.Write(ref _iterationCache, value);
                return value;
            }
        }
    }
}
