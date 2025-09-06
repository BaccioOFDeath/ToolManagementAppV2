using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DeviceManagementApp.Models;

namespace DeviceManagementApp.Interfaces
{
    public interface IDeviceSoftwareService
    {
        Task<IEnumerable<DeviceSoftware>> GetSoftwareAsync(string deviceIp, int? devicePort, CancellationToken cancellationToken = default);
        Task AddOrUpdateAsync(DeviceSoftware software, CancellationToken cancellationToken = default);
        Task DeleteAsync(string deviceIp, int? devicePort, string name, CancellationToken cancellationToken = default);
    }
}
