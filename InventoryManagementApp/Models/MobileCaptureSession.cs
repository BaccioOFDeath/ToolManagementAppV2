using System;

namespace InventoryManagementApp.Models
{
    public sealed class MobileCaptureSession
    {
        public MobileCaptureSession(string url, string token, DateTime expiresAt)
        {
            Url = url;
            Token = token;
            ExpiresAt = expiresAt;
        }

        public string Url { get; }
        public string Token { get; }
        public DateTime ExpiresAt { get; }
    }
}
