using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Models;

namespace InventoryManagementApp.Interfaces
{
    public interface IDeviceDiscoveryService
    {
        Task<IReadOnlyList<DiscoveredDevice>> DiscoverDevicesAsync(CancellationToken cancellationToken = default);
    }
}
