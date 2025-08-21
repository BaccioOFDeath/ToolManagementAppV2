using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using InventoryManagementApp.Utilities.Helpers;
using InventoryManagementApp.Tests;
using Xunit;

namespace InventoryManagementApp.Tests.Utilities
{
    public class PathHelperTests
    {
        [Fact]
        public void GetAbsolutePath_OutsideBaseDir_LogsWarning()
        {
            var logs = new List<LogEntry>();
            using var factory = LoggerFactory.Create(builder => builder.AddProvider(new ListLoggerProvider(logs)));
            var original = PathHelper.Logger;
            PathHelper.Configure(factory.CreateLogger<PathHelper>());
            try
            {
                var result = PathHelper.GetAbsolutePath(Path.Combine("..", "outside.txt"));
                Assert.Null(result);
            }
            finally
            {
                PathHelper.Configure(original);
            }
            Assert.Contains(logs, l => l.Level == LogLevel.Warning);
        }

        [Fact]
        public void GetAbsolutePath_OutsideBaseDir_ThrowsWhenRequested()
        {
            Assert.Throws<InvalidOperationException>(() =>
                PathHelper.GetAbsolutePath(Path.Combine("..", "outside.txt"), true));
        }
    }
}
