using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DeviceManagementApp.Models;

namespace DeviceManagementApp.Interfaces
{
    public interface IAssetService
    {
        Task<IEnumerable<Asset>> GetAssetsAsync(CancellationToken cancellationToken = default);
        Task<Asset?> GetAssetAsync(int assetId, CancellationToken cancellationToken = default);
        Task AddOrUpdateAssetAsync(Asset asset, CancellationToken cancellationToken = default);
        Task DeleteAssetAsync(int assetId, CancellationToken cancellationToken = default);
    }
}
