using System.Windows.Media.Imaging;

namespace ToolManagementAppV2.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string UserPhotoPath { get; set; }
        public bool IsAdmin { get; set; }

        // BitmapImage to bind photos in the UI
        public BitmapImage PhotoBitmap { get; set; }
    }
}
