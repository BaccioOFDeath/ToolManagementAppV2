using System;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using InventoryManagementApp.Models.Domain;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace InventoryManagementApp.Utilities.Converters
{
    public class NullToDefaultImageConverter : IValueConverter
    {
        private static BitmapImage? _defaultItem;
        private static BitmapImage? _defaultLogo;
        private const int MaxCacheEntries = 100;
        private static readonly string[] ItemImageExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
        private static readonly MemoryCache _imageCache = new(new MemoryCacheOptions { SizeLimit = MaxCacheEntries });
        private static readonly MemoryCache _invalidPaths = new(new MemoryCacheOptions { SizeLimit = MaxCacheEntries });
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

            var imagePath = GetImagePath(value);
            if (!string.IsNullOrWhiteSpace(imagePath) && TryLoadImage(imagePath!, out var image))
                return image;

            var itemNumber = GetItemNumber(value);
            if (!string.IsNullOrWhiteSpace(itemNumber))
            {
                foreach (var candidate in BuildItemImageCandidates(itemNumber!))
                    if (TryLoadImage(candidate, out image))
                        return image;
            }

            return GetDefaultImage(parameter);
        }

        private static string? GetImagePath(object value)
        {
            return value switch
            {
                string path => path,
                ItemModel item => item.ImagePath,
                Rental rental => rental.ImagePath,
                Reservation reservation => reservation.ImagePath,
                _ => null
            };
        }

        private static string? GetItemNumber(object value)
        {
            return value switch
            {
                ItemModel item => item.ItemNumber,
                Rental rental => rental.ItemNumber,
                Reservation reservation => reservation.ItemNumber,
                _ => null
            };
        }

        private static IEnumerable<string> BuildItemImageCandidates(string itemNumber)
        {
            var trimmed = itemNumber.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
                yield break;

            foreach (var extension in ItemImageExtensions)
                yield return Path.Combine("Assets", "ItemImages", trimmed + extension);
        }

        public static bool HasCustomImage(object? value)
        {
            var imagePath = value == null ? null : GetImagePath(value);
            if (PathExists(imagePath))
                return true;

            var itemNumber = value == null ? null : GetItemNumber(value);
            if (string.IsNullOrWhiteSpace(itemNumber))
                return false;

            foreach (var candidate in BuildItemImageCandidates(itemNumber))
            {
                if (PathExists(candidate))
                    return true;
            }

            return false;
        }

        private static bool PathExists(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            if (Uri.TryCreate(path, UriKind.Absolute, out var uri) && uri.IsFile)
                return File.Exists(uri.LocalPath);

            var absolutePath = Helpers.PathHelper.GetAbsolutePath(path, false);
            return absolutePath != null && File.Exists(absolutePath);
        }

        private bool TryLoadImage(string path, out BitmapImage image)
        {
            image = null!;
            try
            {
                var cacheKey = path;
                string? absPath;
                if (Uri.IsWellFormedUriString(path, UriKind.Absolute))
                {
                    absPath = path;
                }
                else
                {
                    if (_invalidPaths.TryGetValue(path, out _))
                        return false;

                    absPath = Helpers.PathHelper.GetAbsolutePath(path, false);
                    if (absPath == null || !File.Exists(absPath))
                    {
                        CacheInvalidPath(path);
                        return false;
                    }

                    cacheKey = absPath;
                }

                if (_imageCache.TryGetValue(cacheKey, out BitmapImage? cached) && cached != null)
                {
                    image = cached;
                    return true;
                }

                var loaded = new BitmapImage();
                loaded.BeginInit();
                loaded.CacheOption = BitmapCacheOption.OnLoad;
                loaded.DecodePixelWidth = 256;
                loaded.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                loaded.UriSource = new Uri(absPath, UriKind.Absolute);
                loaded.EndInit();
                loaded.Freeze();

                _imageCache.Set(cacheKey, loaded, new MemoryCacheEntryOptions
                {
                    Size = 1,
                    Priority = CacheItemPriority.Low
                });

                image = loaded;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load image from {Path}", path);
                CacheInvalidPath(path);
                return false;
            }
        }

        private static void CacheInvalidPath(string path)
        {
            _invalidPaths.Set(path, (byte)0, new MemoryCacheEntryOptions
            {
                Size = 1,
                Priority = CacheItemPriority.Low,
                SlidingExpiration = TimeSpan.FromMinutes(5)
            });
        }

        private BitmapImage GetDefaultImage(object parameter)
        {
            string type = (parameter as string)?.ToLowerInvariant() ?? "user";
            switch (type)
            {
                case "item":
                    _defaultItem ??= LoadFromResource("DefaultItemImage.png");
                    return _defaultItem ?? new BitmapImage();

                case "logo":
                    _defaultLogo ??= LoadFromResource("DefaultLogo.png");
                    return _defaultLogo ?? new BitmapImage();

                default:
                    return new BitmapImage();
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
                    return bmp.UriSource?.OriginalString ?? string.Empty;

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
