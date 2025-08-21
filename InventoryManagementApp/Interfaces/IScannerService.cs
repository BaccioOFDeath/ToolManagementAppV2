using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Models;

namespace InventoryManagementApp.Interfaces
{
    public interface IScannerService
    {
        Task<IEnumerable<ScannerDevice>> GetScannerDevicesAsync(CancellationToken cancellationToken);
    }
}
