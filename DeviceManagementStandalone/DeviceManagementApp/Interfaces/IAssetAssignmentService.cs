using System.Threading;
using System.Threading.Tasks;
using DeviceManagementApp.Models;

namespace DeviceManagementApp.Interfaces
{
    public interface IAssetAssignmentService
    {
        Task<AssetAssignment?> GetCurrentAssignmentAsync(int assetId, CancellationToken cancellationToken = default);
        Task AssignAsync(AssetAssignment assignment, CancellationToken cancellationToken = default);
        Task ReturnAsync(int assetId, CancellationToken cancellationToken = default);
    }
}
