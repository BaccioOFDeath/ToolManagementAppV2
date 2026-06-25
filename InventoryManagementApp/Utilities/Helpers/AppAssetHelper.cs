using System;
using System.IO;
using System.Security.Cryptography;

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
            var fullPath = Path.Combine(AppContext.BaseDirectory, AssetsDirectoryName, folderName);
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

        public static string? ResolveAssetPath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            try
            {
                var trimmed = path.Trim();
                return Path.IsPathFullyQualified(trimmed)
                    ? Path.GetFullPath(trimmed)
                    : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, trimmed));
            }
            catch
            {
                return null;
            }
        }

        public static string ToAppRelativePath(string fullPath)
        {
            var relativePath = Path.GetRelativePath(AppContext.BaseDirectory, Path.GetFullPath(fullPath));
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
    }
}
