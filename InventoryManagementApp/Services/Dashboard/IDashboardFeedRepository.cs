using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace InventoryManagementApp.Services.Dashboard
{
    public interface IDashboardFeedRepository
    {
        IAsyncEnumerable<IReadOnlyDictionary<string, object?>> GetRowsAsync(
            DashboardFeedConfig config,
            CancellationToken cancellationToken = default);
    }
}
