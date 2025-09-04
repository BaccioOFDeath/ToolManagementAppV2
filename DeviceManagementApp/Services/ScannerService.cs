using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using DeviceManagementApp.Interfaces;
using DeviceManagementApp.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DeviceManagementApp.Services
{
    public class ScannerService : IScannerService
    {
        private readonly ILogger<ScannerService> _logger;
        private readonly IDeviceService _deviceService;

        public ScannerService(IDeviceService deviceService, ILogger<ScannerService>? logger = null)
        {
            _deviceService = deviceService;
            _logger = logger ?? NullLogger<ScannerService>.Instance;
        }

        public async Task<IEnumerable<Device>> GetDevicesAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var semaphore = new SemaphoreSlim(5);
            var stored = (await _deviceService.GetDevicesAsync(cancellationToken).ConfigureAwait(false)).ToList();
            var tasks = stored.Select(async d =>
            {
                await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var device = new Device
                    {
                        Hostname = string.IsNullOrWhiteSpace(d.Hostname) ? $"Device {d.Ip}" : d.Hostname,
                        Ip = d.Ip,
                        Port = d.Port,
                        Protocol = d.Protocol,
                        Username = d.Username,
                        Password = d.Password,
                        Domain = d.Domain,
                        ItemId = d.ItemId,
                        ItemName = d.ItemName,
                        LastSeen = DateTime.UtcNow.ToLocalTime()
                    };
                    try
                    {
                        using var ping = new Ping();
                        var reply = await ping.SendPingAsync(d.Ip, 1000).ConfigureAwait(false);
                        device.Status = reply.Status == IPStatus.Success ? "Online" : "Offline";
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to ping scanner {Ip}", d.Ip);
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
                .GroupBy(d => new { d.Ip, d.Port })
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
