using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace InventoryManagementApp.Interfaces
{
    public interface IScannerFileService
    {
        Task<IEnumerable<string>> ListFilesAsync(string deviceIp, CancellationToken cancellationToken = default);
    }
}
