using System;
using System.IO;

namespace ToolManagementAppV2.Utilities.Helpers
{
    public static class PathHelper
    {
        public static string GetAbsolutePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return path;

            if (!Path.IsPathRooted(path))
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);

            return Path.GetFullPath(path);
        }
    }
}
