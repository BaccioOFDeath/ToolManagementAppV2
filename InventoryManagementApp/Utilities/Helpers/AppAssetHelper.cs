using System;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using InventoryManagementApp.Utilities;

namespace InventoryManagementApp.Utilities.Helpers
{
    public static class AppAssetHelper
    {
        public const string AssetsDirectoryName = "Assets";
        public const string DataFolder = "Data";
        public const string ItemImagesFolder = "ItemImages";
        public const string RentalPhotosFolder = "RentalPhotos";
        public const string CompanyLogoFolder = "CompanyLogo";
        public const string UserPhotosFolder = "UserPhotos";
        public const string BackgroundsFolder = "Backgrounds";
        public const string ThemesFolder = "Themes";

        private static readonly string[] AllowedImageExtensions = [".png", ".jpg", ".jpeg", ".bmp", ".webp", ".gif"];

        public static string EnsureAssetFolder(string folderName)
        {
            var fullPath = Path.Combine(DeploymentPathResolver.GetDeploymentRoot(AppContext.BaseDirectory), AssetsDirectoryName, folderName);
            Directory.CreateDirectory(fullPath);
            return fullPath;
        }

        public static string CopyImageToAssetFolder(string sourcePath, string folderName, string? fileNameSeed = null)
        {
            if (string.IsNullOrWhiteSpace(sourcePath))
                throw new ArgumentException("Source path is required.", nameof(sourcePath));

            var sourceFullPath = ResolveAssetPath(sourcePath) ?? Path.GetFullPath(sourcePath);
            if (!File.Exists(sourceFullPath))
                throw new FileNotFoundException("Asset source file was not found.", sourcePath);

            var extension = Path.GetExtension(sourceFullPath);
            if (!IsAllowedImageExtension(extension))
                throw new InvalidOperationException("Only image files can be stored in app assets.");

            var targetDirectory = EnsureAssetFolder(folderName);
            var targetDirectoryFullPath = Path.GetFullPath(targetDirectory);
            var sourceFullPathNormalized = Path.GetFullPath(sourceFullPath);
            if (sourceFullPathNormalized.StartsWith(targetDirectoryFullPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                return ToAppRelativePath(sourceFullPathNormalized);

            var safeName = SanitizeFileName(string.IsNullOrWhiteSpace(fileNameSeed)
                ? Path.GetFileNameWithoutExtension(sourceFullPath)
                : fileNameSeed);
            var hash = GetShortHash(sourceFullPathNormalized);
            var targetPath = Path.Combine(targetDirectoryFullPath, $"{safeName}-{hash}{extension.ToLowerInvariant()}");
            File.Copy(sourceFullPathNormalized, targetPath, overwrite: true);
            return ToAppRelativePath(targetPath);
        }

        public static string CopyResizedImageToAssetFolder(
            string sourcePath,
            string folderName,
            int maxWidth,
            int maxHeight,
            string? fileNameSeed = null)
        {
            if (string.IsNullOrWhiteSpace(sourcePath))
                throw new ArgumentException("Source path is required.", nameof(sourcePath));
            if (maxWidth <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxWidth), "Maximum width must be greater than zero.");
            if (maxHeight <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxHeight), "Maximum height must be greater than zero.");

            var sourceFullPath = ResolveAssetPath(sourcePath) ?? Path.GetFullPath(sourcePath);
            if (!File.Exists(sourceFullPath))
                throw new FileNotFoundException("Asset source file was not found.", sourcePath);

            var extension = Path.GetExtension(sourceFullPath);
            if (!IsAllowedImageExtension(extension))
                throw new InvalidOperationException("Only image files can be stored in app assets.");

            var targetDirectory = EnsureAssetFolder(folderName);
            var targetDirectoryFullPath = Path.GetFullPath(targetDirectory);
            var sourceFullPathNormalized = Path.GetFullPath(sourceFullPath);
            if (sourceFullPathNormalized.StartsWith(targetDirectoryFullPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                return ToAppRelativePath(sourceFullPathNormalized);

            var safeName = SanitizeFileName(string.IsNullOrWhiteSpace(fileNameSeed)
                ? Path.GetFileNameWithoutExtension(sourceFullPath)
                : fileNameSeed);
            var hash = GetShortHash(sourceFullPathNormalized);
            var targetPath = Path.Combine(targetDirectoryFullPath, $"{safeName}-{hash}.jpg");
            SaveResizedJpeg(sourceFullPathNormalized, targetPath, maxWidth, maxHeight);
            return ToAppRelativePath(targetPath);
        }

        public static string? ResolveAssetPath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            try
            {
                var trimmed = path.Trim();
                return Path.IsPathFullyQualified(trimmed)
                    ? Path.GetFullPath(trimmed)
                    : Path.GetFullPath(Path.Combine(DeploymentPathResolver.GetDeploymentRoot(AppContext.BaseDirectory), trimmed));
            }
            catch
            {
                return null;
            }
        }

        public static string ToAppRelativePath(string fullPath)
        {
            var relativePath = Path.GetRelativePath(DeploymentPathResolver.GetDeploymentRoot(AppContext.BaseDirectory), Path.GetFullPath(fullPath));
            return relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

        public static bool IsAllowedImageExtension(string? extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
                return false;

            foreach (var allowedExtension in AllowedImageExtensions)
            {
                if (string.Equals(extension, allowedExtension, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        public static string SanitizeFileName(string? value)
        {
            var name = string.IsNullOrWhiteSpace(value) ? "asset" : value.Trim();
            foreach (var invalidChar in Path.GetInvalidFileNameChars())
                name = name.Replace(invalidChar, '-');

            return string.IsNullOrWhiteSpace(name) ? "asset" : name;
        }

        private static string GetShortHash(string path)
        {
            using var stream = File.OpenRead(path);
            return Convert.ToHexString(SHA256.HashData(stream))[..8].ToLowerInvariant();
        }

        private static void SaveResizedJpeg(string sourcePath, string targetPath, int maxWidth, int maxHeight)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(sourcePath, UriKind.Absolute);
            bitmap.EndInit();
            bitmap.Freeze();

            if (bitmap.PixelWidth == 0 || bitmap.PixelHeight == 0)
                throw new InvalidOperationException("Invalid image dimensions: image has zero width or height.");

            var scale = Math.Min((double)maxWidth / bitmap.PixelWidth, (double)maxHeight / bitmap.PixelHeight);
            if (scale > 1.0)
                scale = 1.0;

            BitmapSource source = bitmap;
            if (scale < 1.0)
            {
                source = new TransformedBitmap(bitmap, new ScaleTransform(scale, scale));
                source.Freeze();
            }

            var converted = new FormatConvertedBitmap(source, PixelFormats.Bgr24, null, 0);
            converted.Freeze();

            var encoder = new JpegBitmapEncoder { QualityLevel = 88 };
            encoder.Frames.Add(BitmapFrame.Create(converted));

            using var stream = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None);
            encoder.Save(stream);
        }
    }
}
