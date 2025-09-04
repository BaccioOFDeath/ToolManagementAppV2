using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DeviceManagementApp.Models;

namespace DeviceManagementApp.Interfaces
{
    public interface IDeviceService
    {
        Task<IEnumerable<Device>> GetDevicesAsync(CancellationToken cancellationToken = default);
        Task<Device?> GetDeviceAsync(string ip, int? port, CancellationToken cancellationToken = default);
        Task AddOrUpdateDeviceAsync(Device device, CancellationToken cancellationToken = default);
        Task DeleteDeviceAsync(string ip, int? port, CancellationToken cancellationToken = default);
    }
}
