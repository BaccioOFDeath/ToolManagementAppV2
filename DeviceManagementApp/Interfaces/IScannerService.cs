using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DeviceManagementApp.Models;

namespace DeviceManagementApp.Interfaces
{
    public interface IScannerService
    {
        Task<IEnumerable<Device>> GetDevicesAsync(CancellationToken cancellationToken);
    }
}
