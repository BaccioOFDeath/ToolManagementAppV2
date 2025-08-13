using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace ToolManagementAppV2.Utilities.Helpers
{
    public static class PathHelper
    {
        private static readonly ILogger Logger = App.LoggerFactory.CreateLogger<PathHelper>();

        /// <summary>
        /// Resolves <paramref name="path"/> against the application's base directory
        /// and ensures the resulting absolute path stays within that directory.
        /// </summary>
        /// <param name="path">Relative or absolute path.</param>
        /// <returns>The validated absolute path, or <c>null</c> if validation fails.</returns>
        public static string? GetAbsolutePath(string? path)
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
                    return null;

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
