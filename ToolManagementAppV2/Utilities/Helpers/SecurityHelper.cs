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
        static readonly SemaphoreSlim _iterSemaphore = new(1, 1);

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
            var iterations = GetIterations();
            using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, iterations, HashAlgorithmName.SHA256);
            return Convert.ToBase64String(pbkdf2.GetBytes(32));
        }

        public static async Task<(string hash, string salt)> HashPasswordAsync(string password)
        {
            var saltBytes = new byte[16];
            RandomNumberGenerator.Fill(saltBytes);
            var salt = Convert.ToBase64String(saltBytes);
            var hash = await HashPasswordAsync(password, salt).ConfigureAwait(false);
            return (hash, salt);
        }

        public static async Task<string> HashPasswordAsync(string password, string salt)
        {
            var saltBytes = Convert.FromBase64String(salt);
            var iterations = await GetIterationsAsync().ConfigureAwait(false);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, iterations, HashAlgorithmName.SHA256);
            return Convert.ToBase64String(pbkdf2.GetBytes(32));
        }

        public static bool VerifyPassword(string password, string salt, string hash)
        {
            if (string.IsNullOrEmpty(salt) || string.IsNullOrEmpty(hash)) return false;
            try
            {
                var saltBytes = Convert.FromBase64String(salt);
                var hashBytes = Convert.FromBase64String(hash);
                var iterations = GetIterations();
                using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, iterations, HashAlgorithmName.SHA256);
                var computed = pbkdf2.GetBytes(32);
                return CryptographicOperations.FixedTimeEquals(computed, hashBytes);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public static async Task<bool> VerifyPasswordAsync(string password, string salt, string hash)
        {
            if (string.IsNullOrEmpty(salt) || string.IsNullOrEmpty(hash)) return false;
            try
            {
                var saltBytes = Convert.FromBase64String(salt);
                var hashBytes = Convert.FromBase64String(hash);
                var iterations = await GetIterationsAsync().ConfigureAwait(false);
                using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, iterations, HashAlgorithmName.SHA256);
                var computed = pbkdf2.GetBytes(32);
                return CryptographicOperations.FixedTimeEquals(computed, hashBytes);
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
                    value = svc.GetPasswordIterationsAsync().GetAwaiter().GetResult();
                }

                if (value <= 0)
                    value = DefaultIterations;

                Volatile.Write(ref _iterationCache, value);
                return value;
            }
        }

        static async Task<int> GetIterationsAsync()
        {
            var cached = Volatile.Read(ref _iterationCache);
            if (cached > 0) return cached;

            await _iterSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                cached = _iterationCache;
                if (cached > 0) return cached;

                int value = DefaultIterations;
                var svc = _settingsService;
                if (svc != null)
                {
                    value = await svc.GetPasswordIterationsAsync().ConfigureAwait(false);
                }

                if (value <= 0)
                    value = DefaultIterations;

                Volatile.Write(ref _iterationCache, value);
                return value;
            }
            finally
            {
                _iterSemaphore.Release();
            }
        }
    }
}
