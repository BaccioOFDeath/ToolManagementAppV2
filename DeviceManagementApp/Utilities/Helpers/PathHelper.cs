using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DeviceManagementApp.Utilities.Helpers
{
    public static class PathHelper
    {
        public static ILogger Logger { get; private set; } = NullLogger.Instance;

        public static void Configure(ILogger logger)
            => Logger = logger ?? NullLogger.Instance;

        /// <summary>
        /// Resolves <paramref name="path"/> against the application's base directory
        /// and ensures the resulting absolute path stays within that directory.
        /// </summary>
        /// <param name="path">Relative or absolute path.</param>
        /// <param name="throwOnInvalid">Throw if the path resolves outside the application's base directory.</param>
        /// <returns>The validated absolute path, or <c>null</c> if validation fails.</returns>
        public static string? GetAbsolutePath(string? path, bool throwOnInvalid = false)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            try
            {
                var baseDir = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);
                var combined = Path.IsPathRooted(path)
                    ? path
                    : Path.Combine(baseDir, path);

                var fullPath = Path.GetFullPath(combined);

                if (!fullPath.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase))
                {
                    Logger.LogWarning("Resolved path {Path} is outside base directory {BaseDir}", fullPath, baseDir);
                    if (throwOnInvalid)
                        throw new InvalidOperationException("Path is outside of the application's base directory.");
                    return null;
                }

                return fullPath;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to resolve path {Path}", path);
                return null;
            }
        }
    }
}
