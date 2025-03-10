// Revised NullToDefaultImageConverter.cs
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ToolManagementAppV2.Utilities
{
    public class NullToDefaultImageConverter : IValueConverter
    {
        private static BitmapImage _defaultImage;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BitmapImage bitmap)
                return bitmap;

            string path = value as string;
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    return new BitmapImage(new Uri(path, UriKind.Absolute));
                }
                catch { }
            }
            if (_defaultImage == null)
            {
                try
                {
                    string defaultFile = "DefaultUserPhoto.png";
                    if (parameter != null)
                    {
                        string param = parameter.ToString();
                        if (param.Equals("Tool", StringComparison.OrdinalIgnoreCase))
                            defaultFile = "DefaultToolImage.png";
                        else if (param.Equals("Logo", StringComparison.OrdinalIgnoreCase))
                            defaultFile = "DefaultLogo.png";
                    }
                    var resourceUri = new Uri($"pack://application:,,,/Resources/{defaultFile}", UriKind.Absolute);
                    _defaultImage = new BitmapImage(resourceUri);
                }
                catch
                {
                    _defaultImage = new BitmapImage();
                }
            }
            return _defaultImage;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
