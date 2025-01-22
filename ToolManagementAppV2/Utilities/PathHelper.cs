using System;
using System.IO;

namespace ToolManagementAppV2.Utilities
{
    public static class PathHelper
    {
        public static string GetAbsolutePath(string relativePath)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
        }
    }
}
