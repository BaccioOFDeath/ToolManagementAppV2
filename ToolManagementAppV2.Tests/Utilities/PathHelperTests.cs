using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using ToolManagementAppV2.Utilities.Helpers;
using ToolManagementAppV2.Tests;
using Xunit;

namespace ToolManagementAppV2.Tests.Utilities
{
    public class PathHelperTests
    {
        [Fact]
        public void GetAbsolutePath_OutsideBaseDir_LogsWarning()
        {
            var logs = new List<LogEntry>();
            var factory = LoggerFactory.Create(builder => builder.AddProvider(new ListLoggerProvider(logs)));
            var originalFactory = App.LoggerFactory;
            App.LoggerFactory = factory;
            try
            {
                var result = PathHelper.GetAbsolutePath(Path.Combine("..", "outside.txt"));
                Assert.Null(result);
            }
            finally
            {
                App.LoggerFactory = originalFactory;
                factory.Dispose();
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
