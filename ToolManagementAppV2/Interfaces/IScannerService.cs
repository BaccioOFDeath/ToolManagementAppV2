using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ToolManagementAppV2.Models;

namespace ToolManagementAppV2.Interfaces
{
    public interface IScannerService
    {
        Task<IEnumerable<ScannerDevice>> GetScannerDevicesAsync(CancellationToken cancellationToken);
    }
}
