using InventoryManagementApp.Models;
using System;
using System.IO;
using System.Security.Cryptography;

namespace InventoryManagementApp.Utilities.Helpers
{
    public static class ThemeBackgroundAssetHelper
    {
        public static readonly string RelativeBackgroundDirectory = Path.Combine(AppAssetHelper.AssetsDirectoryName, AppAssetHelper.BackgroundsFolder);

        public static string AppBackgroundDirectory => AppAssetHelper.EnsureAssetFolder(AppAssetHelper.BackgroundsFolder);

        public static string? ResolveBackgroundImagePath(string? imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                return null;

            return AppAssetHelper.ResolveAssetPath(imagePath);
        }

        public static string CopyToAppAssets(string? sourcePath)
        {
            var resolvedSource = ResolveBackgroundImagePath(sourcePath);
            if (string.IsNullOrWhiteSpace(resolvedSource) || !File.Exists(resolvedSource))
                return sourcePath ?? string.Empty;

            if (!AppAssetHelper.IsAllowedImageExtension(Path.GetExtension(resolvedSource)))
                return sourcePath ?? string.Empty;

            return AppAssetHelper.CopyImageToAssetFolder(resolvedSource, AppAssetHelper.BackgroundsFolder, Path.GetFileNameWithoutExtension(resolvedSource));
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

            var fileName = AppAssetHelper.SanitizeFileName(Path.GetFileNameWithoutExtension(settings.BackgroundImageFileName));
            var extension = Path.GetExtension(settings.BackgroundImageFileName);
            if (!AppAssetHelper.IsAllowedImageExtension(extension))
            {
                extension = Path.GetExtension(settings.BackgroundImagePath);
            }

            if (!AppAssetHelper.IsAllowedImageExtension(extension))
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
            settings.BackgroundImagePath = AppAssetHelper.ToAppRelativePath(targetPath);
            settings.BackgroundImageFileName = null;
            settings.BackgroundImageContentBase64 = null;
        }
    }
}
