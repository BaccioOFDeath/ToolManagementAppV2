using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Models;

namespace InventoryManagementApp.Interfaces
{
    public interface IDeviceDiscoveryService
    {
        Task<IReadOnlyList<DiscoveredDevice>> DiscoverDevicesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Indicates whether any subnets have been configured for device discovery.
        /// </summary>
        bool HasConfiguredSubnets { get; }
    }
}
