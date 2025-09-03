using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Models;

namespace InventoryManagementApp.Interfaces
{
    public interface IDeviceDiscoveryService
    {
        Task<IReadOnlyList<DiscoveredDevice>> DiscoverDevicesAsync(CancellationToken cancellationToken = default);

        IAsyncEnumerable<DiscoveredDevice> DiscoverDevicesAsync(IProgress<double>? progress = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default);

        /// <summary>
        /// Indicates whether any subnets have been configured for device discovery.
        /// </summary>
        bool HasConfiguredSubnets { get; }
    }
}
