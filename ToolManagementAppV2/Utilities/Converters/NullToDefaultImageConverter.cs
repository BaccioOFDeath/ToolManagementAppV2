// Revised NullToDefaultImageConverter.cs
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ToolManagementAppV2.Utilities.Converters
{
    public class NullToDefaultImageConverter : IValueConverter
    {
        private static BitmapImage _defaultUser;
        private static BitmapImage _defaultTool;
        private static BitmapImage _defaultLogo;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // If we've already got an actual BitmapImage, just return it
            if (value is BitmapImage bmp) return bmp;

            // If it's a path, try loading it
            if (value is string path && !string.IsNullOrEmpty(path))
            {
                try { return new BitmapImage(new Uri(path, UriKind.Absolute)); }
                catch { /* fall‐through to default */ }
            }

            // Figure out which default we need
            string type = (parameter as string)?.ToLowerInvariant() ?? "user";
            switch (type)
            {
                case "tool":
                    if (_defaultTool == null)
                        _defaultTool = LoadFromResource("DefaultToolImage.png");
                    return _defaultTool;

                case "logo":
                    if (_defaultLogo == null)
                        _defaultLogo = LoadFromResource("DefaultLogo.png");
                    return _defaultLogo;

                default: // user
                    if (_defaultUser == null)
                        _defaultUser = LoadFromResource("DefaultUserPhoto.png");
                    return _defaultUser;
            }
        }

        private BitmapImage LoadFromResource(string fileName)
        {
            try
            {
                var uri = new Uri($"pack://application:,,,/Resources/{fileName}", UriKind.Absolute);
                return new BitmapImage(uri);
            }
            catch
            {
                return new BitmapImage(); // empty fallback
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
