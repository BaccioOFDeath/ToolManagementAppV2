using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Documents;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Services.Tools;
using Xunit;
using ToolModel = ToolManagementAppV2.Models.Domain.Tool;

namespace ToolManagementAppV2.Tests.Services
{
    public class PrinterTests
    {
        private class DummySettingsService : ISettingsService
        {
            public void SaveSetting(string key, string value) { }
            public string? GetSetting(string key) => null;
            public Dictionary<string, string> GetAllSettings() => new();
            public void UpdateSettings(Dictionary<string, string> settings) { }
            public void DeleteSetting(string key) { }
            public IEnumerable<string> GetScannerIpAddresses() => Enumerable.Empty<string>();
            public IEnumerable<string> SaveScannerIpAddresses(IEnumerable<string>? ipAddresses) => Enumerable.Empty<string>();
            public int GetPasswordIterations() => 0;
            public void SavePasswordIterations(int iterations) { }
        }

        [Fact]
        public async Task BuildDocumentIncrementallyAsync_ProcessesAllTools()
        {
            var printer = new Printer(new DummySettingsService());
            var tools = Enumerable.Range(0, 120).Select(i => new ToolModel
            {
                ToolNumber = $"T{i}",
                Location = i.ToString()
            });

            var method = typeof(Printer).GetMethod("BuildDocumentIncrementallyAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            var task = (Task<FlowDocument>)method.Invoke(printer, new object[] { tools, "Title", null, null, 25 });
            var doc = await task;

            Assert.Equal(1 + 120, doc.Blocks.Count);
        }
    }
}
