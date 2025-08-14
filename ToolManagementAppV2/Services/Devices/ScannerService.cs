using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ToolManagementAppV2.Services.Devices
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

        public IEnumerable<ScannerDevice> GetScannerDevices()
        {
            var list = new List<ScannerDevice>();
            foreach (var ip in _settingsService.GetScannerIpAddresses())
            {
                var device = new ScannerDevice
                {
                    Name = $"Scanner {ip}",
                    Ip = ip,
                    LastSeen = DateTime.Now
                };
                try
                {
                    using var ping = new Ping();
                    var reply = ping.Send(ip, 1000);
                    device.Status = reply.Status == IPStatus.Success ? "Online" : "Offline";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to ping scanner {Ip}", ip);
                    device.Status = "Error";
                }
                list.Add(device);
            }
            return list;
        }
    }
}
