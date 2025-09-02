using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace InventoryManagementApp.Services.Devices
{
    public class ScannerFileService : IScannerFileService
    {
        private readonly ILogger<ScannerFileService> _logger;

        public ScannerFileService(ILogger<ScannerFileService>? logger = null)
        {
            _logger = logger ?? NullLogger<ScannerFileService>.Instance;
        }

        public Task<IEnumerable<string>> ListFilesAsync(string deviceIp, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var path = $"\\\\{deviceIp}\\Shared";
                if (!Directory.Exists(path))
                    return Task.FromResult<IEnumerable<string>>(Array.Empty<string>());
                var files = Directory.GetFiles(path)
                                     .Select(Path.GetFileName)!
                                     .ToArray();
                return Task.FromResult<IEnumerable<string>>(files);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list files for device {Ip}", deviceIp);
                return Task.FromResult<IEnumerable<string>>(Array.Empty<string>());
            }
        }
    }
}
