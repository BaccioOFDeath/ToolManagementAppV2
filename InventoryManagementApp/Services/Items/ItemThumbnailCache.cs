using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Utilities.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace InventoryManagementApp.Services.Items;

/// <summary>
/// Builds small, frozen item thumbnails away from the UI thread and stores them
/// on disk so later application sessions do not decode the original photographs.
/// </summary>
public sealed class ItemThumbnailCache
{
    private const int ThumbnailPixelWidth = 128;
    private const int MaxMemoryEntries = 200;
    private const int MaxPersistentEntries = 1000;
    private static readonly string[] ItemImageExtensions = [".jpg", ".jpeg", ".png", ".bmp", ".gif"];
    private readonly ConcurrentDictionary<string, ImageSource> _memoryCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly SemaphoreSlim _imageIoGate = new(2, 2);
    private readonly ILogger<ItemThumbnailCache> _logger;
    private readonly string _cacheDirectory;
    private int _cleanupStarted;

    public ItemThumbnailCache(ILogger<ItemThumbnailCache>? logger = null)
        : this(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "InventoryManagementApp", "ThumbnailCache"), logger)
    {
    }

    internal ItemThumbnailCache(string cacheDirectory, ILogger<ItemThumbnailCache>? logger = null)
    {
        _cacheDirectory = cacheDirectory;
        _logger = logger ?? NullLogger<ItemThumbnailCache>.Instance;
    }

    public Task<ImageSource?> GetAsync(ItemModel item, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);
        return GetCoreAsync(item.ImagePath, item.ItemNumber, cancellationToken);
    }

    private async Task<ImageSource?> GetCoreAsync(string? imagePath, string? itemNumber, CancellationToken cancellationToken)
    {
        await _imageIoGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                var sourcePath = ResolveSourcePath(imagePath, itemNumber);
                if (sourcePath is null)
                    return null;

                var file = new FileInfo(sourcePath);
                var cacheKey = BuildCacheKey(file);
                if (_memoryCache.TryGetValue(cacheKey, out var memoryImage))
                    return memoryImage;

                var image = LoadOrCreateThumbnail(file, cacheKey, cancellationToken);
                if (image is null)
                    return null;

                _memoryCache[cacheKey] = image;
                var trimCount = Math.Max(0, _memoryCache.Count - MaxMemoryEntries);
                foreach (var oldKey in _memoryCache.Keys.Take(trimCount))
                    _memoryCache.TryRemove(oldKey, out _);
                return image;
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to resolve or create an item thumbnail");
            return null;
        }
        finally
        {
            _imageIoGate.Release();
        }
    }

    private ImageSource? LoadOrCreateThumbnail(FileInfo source, string cacheKey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Directory.CreateDirectory(_cacheDirectory);
        StartCleanup();
        var cachePath = Path.Combine(_cacheDirectory, cacheKey + ".png");
        if (File.Exists(cachePath))
            return LoadFrozenBitmap(cachePath, null);

        var thumbnail = LoadFrozenBitmap(source.FullName, ThumbnailPixelWidth);
        if (thumbnail is null)
            return null;

        cancellationToken.ThrowIfCancellationRequested();
        var temporaryPath = cachePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(thumbnail));
            using (var stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                encoder.Save(stream);
            File.Move(temporaryPath, cachePath, true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }

        return thumbnail;
    }

    private void StartCleanup()
    {
        if (Interlocked.Exchange(ref _cleanupStarted, 1) != 0)
            return;

        _ = Task.Run(() =>
        {
            try
            {
                var files = new DirectoryInfo(_cacheDirectory).EnumerateFiles("*.png")
                    .OrderByDescending(file => file.LastAccessTimeUtc)
                    .Skip(MaxPersistentEntries)
                    .ToArray();
                foreach (var file in files)
                    file.Delete();
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Unable to trim the item thumbnail cache");
            }
        });
    }

    private static BitmapImage? LoadFrozenBitmap(string path, int? decodePixelWidth)
    {
        try
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            if (decodePixelWidth.HasValue)
                image.DecodePixelWidth = decodePixelWidth.Value;
            image.UriSource = new Uri(path, UriKind.Absolute);
            image.EndInit();
            image.Freeze();
            return image;
        }
        catch
        {
            return null;
        }
    }

    private static string BuildCacheKey(FileInfo file)
    {
        var identity = $"{file.FullName}|{file.LastWriteTimeUtc.Ticks}|{file.Length}|{ThumbnailPixelWidth}";
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(identity)));
    }

    private static string? ResolveSourcePath(string? imagePath, string? itemNumberValue)
    {
        if (!string.IsNullOrWhiteSpace(imagePath))
        {
            var resolved = PathHelper.GetAbsolutePath(imagePath, false) ?? AppAssetHelper.ResolveAssetPath(imagePath);
            if (!string.IsNullOrWhiteSpace(resolved) && File.Exists(resolved))
                return Path.GetFullPath(resolved);
        }

        var itemNumber = itemNumberValue?.Trim();
        if (string.IsNullOrWhiteSpace(itemNumber))
            return null;

        foreach (var extension in ItemImageExtensions)
        {
            var candidate = Path.Combine(AppAssetHelper.AssetsDirectoryName, AppAssetHelper.ItemImagesFolder, itemNumber + extension);
            var resolved = PathHelper.GetAbsolutePath(candidate, false) ?? AppAssetHelper.ResolveAssetPath(candidate);
            if (!string.IsNullOrWhiteSpace(resolved) && File.Exists(resolved))
                return Path.GetFullPath(resolved);
        }

        return null;
    }
}
