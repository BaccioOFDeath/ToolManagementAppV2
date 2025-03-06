using System.Windows.Media.Imaging;

// File: Models/User.cs
namespace ToolManagementAppV2.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string UserPhotoPath { get; set; }
        public bool IsAdmin { get; set; }
        public BitmapImage PhotoBitmap { get; set; }
    }
}

