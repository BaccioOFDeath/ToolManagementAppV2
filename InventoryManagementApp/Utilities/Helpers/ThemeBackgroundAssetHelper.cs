using InventoryManagementApp.Models;
using System;
using System.IO;
using System.Security.Cryptography;

namespace InventoryManagementApp.Utilities.Helpers
{
    public static class ThemeBackgroundAssetHelper
    {
        public static readonly string RelativeBackgroundDirectory = Path.Combine("Assets", "Backgrounds");
        private static readonly string[] AllowedExtensions = [".png", ".jpg", ".jpeg", ".bmp", ".webp"];

        public static string AppBackgroundDirectory => Path.Combine(AppContext.BaseDirectory, RelativeBackgroundDirectory);

        public static string? ResolveBackgroundImagePath(string? imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                return null;

            var trimmed = imagePath.Trim();
            if (Path.IsPathFullyQualified(trimmed))
                return trimmed;

            return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, trimmed));
        }

        public static string CopyToAppAssets(string? sourcePath)
        {
            var resolvedSource = ResolveBackgroundImagePath(sourcePath);
            if (string.IsNullOrWhiteSpace(resolvedSource) || !File.Exists(resolvedSource))
                return sourcePath ?? string.Empty;

            var extension = Path.GetExtension(resolvedSource);
            if (!IsAllowedImageExtension(extension))
                return sourcePath ?? string.Empty;

            Directory.CreateDirectory(AppBackgroundDirectory);

            var appBackgroundDirectory = Path.GetFullPath(AppBackgroundDirectory);
            var sourceFullPath = Path.GetFullPath(resolvedSource);
            if (sourceFullPath.StartsWith(appBackgroundDirectory + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                return ToAppRelativePath(sourceFullPath);

            var fileNameWithoutExtension = SanitizeFileName(Path.GetFileNameWithoutExtension(resolvedSource));
            var hash = GetShortHash(resolvedSource);
            var targetPath = Path.Combine(appBackgroundDirectory, $"{fileNameWithoutExtension}-{hash}{extension.ToLowerInvariant()}");
            File.Copy(sourceFullPath, targetPath, overwrite: true);
            return ToAppRelativePath(targetPath);
        }

        public static void AddEmbeddedBackground(AppThemeSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            settings.BackgroundImageFileName = null;
            settings.BackgroundImageContentBase64 = null;

            var backgroundPath = ResolveBackgroundImagePath(settings.BackgroundImagePath);
            if (string.IsNullOrWhiteSpace(backgroundPath) || !File.Exists(backgroundPath))
                return;

            settings.BackgroundImageFileName = Path.GetFileName(backgroundPath);
            settings.BackgroundImageContentBase64 = Convert.ToBase64String(File.ReadAllBytes(backgroundPath));
        }

        public static void ExtractEmbeddedBackground(AppThemeSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            if (string.IsNullOrWhiteSpace(settings.BackgroundImageContentBase64))
                return;

            var fileName = SanitizeFileName(Path.GetFileNameWithoutExtension(settings.BackgroundImageFileName));
            var extension = Path.GetExtension(settings.BackgroundImageFileName);
            if (!IsAllowedImageExtension(extension))
            {
                extension = Path.GetExtension(settings.BackgroundImagePath);
            }

            if (!IsAllowedImageExtension(extension))
                extension = ".png";
            extension ??= ".png";

            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(settings.BackgroundImageContentBase64);
            }
            catch (FormatException)
            {
                return;
            }

            Directory.CreateDirectory(AppBackgroundDirectory);
            var hash = Convert.ToHexString(SHA256.HashData(bytes))[..8].ToLowerInvariant();
            var targetPath = Path.Combine(AppBackgroundDirectory, $"{fileName}-{hash}{extension.ToLowerInvariant()}");
            File.WriteAllBytes(targetPath, bytes);
            settings.BackgroundImagePath = ToAppRelativePath(targetPath);
            settings.BackgroundImageFileName = null;
            settings.BackgroundImageContentBase64 = null;
        }

        private static string ToAppRelativePath(string fullPath)
        {
            var relativePath = Path.GetRelativePath(AppContext.BaseDirectory, fullPath);
            return relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

        private static string SanitizeFileName(string? value)
        {
            var name = string.IsNullOrWhiteSpace(value) ? "background" : value.Trim();
            foreach (var invalidChar in Path.GetInvalidFileNameChars())
                name = name.Replace(invalidChar, '-');

            return string.IsNullOrWhiteSpace(name) ? "background" : name;
        }

        private static bool IsAllowedImageExtension(string? extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
                return false;

            foreach (var allowedExtension in AllowedExtensions)
            {
                if (string.Equals(extension, allowedExtension, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static string GetShortHash(string path)
        {
            using var stream = File.OpenRead(path);
            return Convert.ToHexString(SHA256.HashData(stream))[..8].ToLowerInvariant();
        }
    }
}
