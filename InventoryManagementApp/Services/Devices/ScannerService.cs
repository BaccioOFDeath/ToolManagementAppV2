using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace InventoryManagementApp.Services.Devices
{
    public class ScannerService : IScannerService
    {
        private readonly ILogger<ScannerService> _logger;
        private readonly ISettingsService _settingsService;

        public ScannerService(ISettingsService settingsService, ILogger<ScannerService>? logger = null)
        {
            _settingsService = settingsService;
            _logger = logger ?? NullLogger<ScannerService>.Instance;
        }

        public async Task<IEnumerable<ScannerDevice>> GetScannerDevicesAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var semaphore = new SemaphoreSlim(5);
            var ips = (await _settingsService.GetScannerIpAddressesAsync(cancellationToken).ConfigureAwait(false)).Distinct();
            var tasks = ips.Select(async ip =>
            {
                await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var device = new ScannerDevice
                    {
                        Name = $"Scanner {ip}",
                        Ip = ip,
                        LastSeen = DateTime.UtcNow
                    };
                    try
                    {
                        using var ping = new Ping();
                        var reply = await ping.SendPingAsync(ip, 1000).ConfigureAwait(false);
                        device.Status = reply.Status == IPStatus.Success ? "Online" : "Offline";
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to ping scanner {Ip}", ip);
                        device.Status = "Error";
                    }
                    return device;
                }
                finally
                {
                    semaphore.Release();
                }
            });

            var devices = await Task.WhenAll(tasks).ConfigureAwait(false);

            // Merge statuses for any duplicate IPs
            return devices
                .GroupBy(d => d.Ip)
                .Select(g =>
                {
                    var device = g.First();
                    if (g.Any(d => d.Status == "Online"))
                        device.Status = "Online";
                    else if (g.Any(d => d.Status == "Offline"))
                        device.Status = "Offline";
                    else
                        device.Status = "Error";
                    return device;
                });
        }
    }
}
