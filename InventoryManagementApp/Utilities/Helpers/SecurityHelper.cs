using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Interfaces;

namespace InventoryManagementApp.Utilities.Helpers
{
    /// <summary>
    /// Provides secure password hashing, verification, and generation utilities using PBKDF2-SHA256.
    /// </summary>
    public static class SecurityHelper
    {
        private const int DefaultIterations = 100_000;
        private static ISettingsService? _settingsService;
        private static int _iterationCache;
        private static readonly SemaphoreSlim _iterSemaphore = new(1, 1);

        /// <summary>
        /// Gets or sets the settings service used to retrieve password iteration count.
        /// Setting this will invalidate the iteration cache.
        /// </summary>
        public static ISettingsService? SettingsService
        {
            get => _settingsService;
            set
            {
                _settingsService = value;
                Volatile.Write(ref _iterationCache, 0);
            }
        }

        private const string PasswordChars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@#$%^&*";

        /// <summary>
        /// Determines whether a string is a valid SHA256 hash (64 hexadecimal characters).
        /// </summary>
        /// <param name="input">The string to validate.</param>
        /// <returns>True if the input is a valid SHA256 hash; otherwise, false.</returns>
        public static bool IsSha256Hash(string input)
        {
            if (string.IsNullOrWhiteSpace(input) || input.Length != 64) return false;
            foreach (var c in input) if (!Uri.IsHexDigit(c)) return false;
            return true;
        }

        /// <summary>
        /// Hashes a password with a newly generated salt using PBKDF2-SHA256.
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <param name="salt">Output parameter containing the generated salt.</param>
        /// <returns>The hashed password.</returns>
        public static string HashPassword(string password, out string salt)
        {
            var saltBytes = new byte[16];
            RandomNumberGenerator.Fill(saltBytes);
            salt = Convert.ToBase64String(saltBytes);
            return HashPassword(password, salt);
        }

        /// <summary>
        /// Hashes a password with an existing salt using PBKDF2-SHA256.
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <param name="salt">The salt to use for hashing.</param>
        /// <returns>The hashed password.</returns>
        public static string HashPassword(string password, string salt)
        {
            var saltBytes = Convert.FromBase64String(salt);
            var iterations = GetCachedIterations();
            using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, iterations, HashAlgorithmName.SHA256);
            return Convert.ToBase64String(pbkdf2.GetBytes(32));
        }

        /// <summary>
        /// Asynchronously hashes a password with a newly generated salt using PBKDF2-SHA256.
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <returns>A tuple containing the hash and salt.</returns>
        public static async Task<(string hash, string salt)> HashPasswordAsync(string password)
        {
            var saltBytes = new byte[16];
            RandomNumberGenerator.Fill(saltBytes);
            var salt = Convert.ToBase64String(saltBytes);
            var hash = await HashPasswordAsync(password, salt).ConfigureAwait(false);
            return (hash, salt);
        }

        /// <summary>
        /// Asynchronously hashes a password with an existing salt using PBKDF2-SHA256.
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <param name="salt">The salt to use for hashing.</param>
        /// <returns>The hashed password.</returns>
        public static async Task<string> HashPasswordAsync(string password, string salt)
        {
            var saltBytes = Convert.FromBase64String(salt);
            var iterations = await GetIterationsAsync().ConfigureAwait(false);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, iterations, HashAlgorithmName.SHA256);
            return Convert.ToBase64String(pbkdf2.GetBytes(32));
        }

        /// <summary>
        /// Verifies a password against a hash and salt using constant-time comparison.
        /// </summary>
        /// <param name="password">The password to verify.</param>
        /// <param name="salt">The salt used for hashing.</param>
        /// <param name="hash">The expected hash.</param>
        /// <returns>True if the password matches; otherwise, false.</returns>
        public static bool VerifyPassword(string password, string salt, string hash)
        {
            if (string.IsNullOrEmpty(salt) || string.IsNullOrEmpty(hash)) return false;
            try
            {
                var saltBytes = Convert.FromBase64String(salt);
                var hashBytes = Convert.FromBase64String(hash);
                var iterations = GetCachedIterations();
                using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, iterations, HashAlgorithmName.SHA256);
                var computed = pbkdf2.GetBytes(32);
                return CryptographicOperations.FixedTimeEquals(computed, hashBytes);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        /// <summary>
        /// Asynchronously verifies a password against a hash and salt using constant-time comparison.
        /// </summary>
        /// <param name="password">The password to verify.</param>
        /// <param name="salt">The salt used for hashing.</param>
        /// <param name="hash">The expected hash.</param>
        /// <returns>True if the password matches; otherwise, false.</returns>
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

        /// <summary>
        /// Generates a cryptographically secure random password.
        /// </summary>
        /// <param name="length">The length of the password to generate (default: 12).</param>
        /// <returns>A randomly generated password.</returns>
        public static string GeneratePassword(int length = 12)
        {
            if (length < 1)
                throw new ArgumentOutOfRangeException(nameof(length), "Password length must be at least 1.");
            
            var bytes = new byte[length];
            RandomNumberGenerator.Fill(bytes);
            var chars = new char[length];
            for (int i = 0; i < length; i++) chars[i] = PasswordChars[bytes[i] % PasswordChars.Length];
            return new string(chars);
        }

        /// <summary>
        /// Computes a SHA256 hash of the input string (legacy method for backward compatibility).
        /// </summary>
        /// <param name="rawData">The data to hash.</param>
        /// <returns>The hexadecimal string representation of the hash.</returns>
        public static string ComputeSha256HashLegacy(string rawData)
        {
            using var sha256Hash = SHA256.Create();
            var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        /// <summary>
        /// Gets the cached iteration count for password hashing, returning the default if not cached.
        /// </summary>
        /// <returns>The number of iterations to use for password hashing.</returns>
        private static int GetCachedIterations()
        {
            var cached = Volatile.Read(ref _iterationCache);
            return cached > 0 ? cached : DefaultIterations;
        }

        /// <summary>
        /// Asynchronously retrieves the iteration count from settings, with caching to improve performance.
        /// </summary>
        /// <returns>The number of iterations to use for password hashing.</returns>
        internal static async Task<int> GetIterationsAsync()
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
