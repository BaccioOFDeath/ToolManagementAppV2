// Revised NullToDefaultImageConverter.cs
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.IO;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace InventoryManagementApp.Utilities.Converters
{
    public class NullToDefaultImageConverter : IValueConverter
    {
        private static BitmapImage _defaultUser;
        private static BitmapImage _defaultItem;
        private static BitmapImage _defaultLogo;
        private const int MaxCacheEntries = 100;
        private static readonly ConcurrentDictionary<string, BitmapImage> _imageCache =
            new ConcurrentDictionary<string, BitmapImage>(StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentQueue<string> _cacheOrder = new ConcurrentQueue<string>();
        private static readonly ConcurrentDictionary<string, byte> _invalidPaths =
            new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);
        private readonly ILogger<NullToDefaultImageConverter> _logger;

        public NullToDefaultImageConverter() : this(null) { }

        public NullToDefaultImageConverter(ILogger<NullToDefaultImageConverter>? logger = null)
        {
            _logger = logger ?? NullLogger<NullToDefaultImageConverter>.Instance;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // If we've already got an actual BitmapImage, just return it
            if (value is BitmapImage bmp) return bmp;

            // If it's a path, try loading it
            if (value is string path && !string.IsNullOrEmpty(path))
            {
                try
                {
                    string absPath;
                    if (Uri.IsWellFormedUriString(path, UriKind.Absolute))
                    {
                        absPath = path;
                    }
                    else
                    {
                        if (_invalidPaths.ContainsKey(path))
                            return GetDefaultImage(parameter);

                        absPath = Helpers.PathHelper.GetAbsolutePath(path, false);
                        if (string.IsNullOrEmpty(absPath))
                        {
                            _invalidPaths.TryAdd(path, 0);
                            return GetDefaultImage(parameter);
                        }
                    }

                    if (_imageCache.TryGetValue(absPath, out var cached))
                        return cached;

                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.UriSource = new Uri(absPath, UriKind.Absolute);
                    image.EndInit();
                    image.Freeze();

                    if (_imageCache.TryAdd(absPath, image))
                    {
                        _cacheOrder.Enqueue(absPath);
                        while (_cacheOrder.Count > MaxCacheEntries && _cacheOrder.TryDequeue(out var oldKey))
                        {
                            _imageCache.TryRemove(oldKey, out _);
                        }
                    }

                    return image;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load image from {Path}", path);
                    _invalidPaths.TryAdd(path, 0);
                    // fall-through to default
                }
            }

            return GetDefaultImage(parameter);
        }

        private BitmapImage GetDefaultImage(object parameter)
        {
            string type = (parameter as string)?.ToLowerInvariant() ?? "user";
            switch (type)
            {
                case "item":
                    if (_defaultItem == null)
                        _defaultItem = LoadFromResource("DefaultItemImage.png");
                    return _defaultItem;

                case "logo":
                    if (_defaultLogo == null)
                        _defaultLogo = LoadFromResource("DefaultLogo.png");
                    return _defaultLogo;

                default:
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load resource {FileName}", fileName);
                return new BitmapImage(); // empty fallback
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is BitmapImage bmp)
                    return bmp.UriSource?.OriginalString;

                if (value is string path)
                    return path;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ConvertBack failed");
            }

            return System.Windows.Data.Binding.DoNothing;
        }
    }
}
