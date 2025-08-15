using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using ToolManagementAppV2.Utilities.Helpers;
using Xunit;

namespace ToolManagementAppV2.Tests
{
    public class Utilities
    {
        [Fact]
        public void GetAbsolutePath_RelativePathInsideApp_ReturnsAbsolutePath()
        {
            var baseDir = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);
            var result = PathHelper.GetAbsolutePath("testfile.tmp");
            var expected = Path.GetFullPath(Path.Combine(baseDir, "testfile.tmp"));
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetAbsolutePath_PathTraversal_ReturnsNull()
        {
            var result = PathHelper.GetAbsolutePath(".." + Path.DirectorySeparatorChar + "file.txt");
            Assert.Null(result);
        }

        [Fact]
        public void GetAbsolutePath_InvalidCharacters_ReturnsNullAndLogs()
        {
            var logs = new List<LogEntry>();
            using var factory = LoggerFactory.Create(builder => builder.AddProvider(new ListLoggerProvider(logs)));
            var original = PathHelper.Logger;
            PathHelper.Configure(factory.CreateLogger<PathHelper>());
            try
            {
                var result = PathHelper.GetAbsolutePath("invalid|path");
                Assert.Null(result);
            }
            finally
            {
                PathHelper.Configure(original);
            }
            Assert.Contains(logs, l => l.Level == LogLevel.Error);
        }
    }
}
