using System.Security.Cryptography;
using System.Text;

namespace ToolManagementAppV2.Utilities.Helpers
{
    public static class SecurityHelper
    {
        const int Iterations = 100_000;

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
            using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithmName.SHA256);
            return Convert.ToBase64String(pbkdf2.GetBytes(32));
        }

        public static bool VerifyPassword(string password, string salt, string hash)
        {
            if (string.IsNullOrEmpty(salt))
                return false;

            var computed = HashPassword(password, salt);
            return computed == hash;
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
    }
}
