using System.Security.Cryptography;
using System.Text;

namespace ToolManagementAppV2.Utilities.Helpers
{
    public static class SecurityHelper
    {
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

        public static string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2")); // lowercase hex
                }
                return builder.ToString();
            }
        }
    }
}
