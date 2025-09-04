using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DeviceManagementApp.Models;

namespace DeviceManagementApp.Interfaces
{
    public interface IDeviceFileService
    {
        Task<IEnumerable<string>> ListFilesAsync(Device device, string? extensionFilter = null, CancellationToken cancellationToken = default);
        Task<int> DownloadUnseenFilesAsync(Device device, string basePath, CancellationToken cancellationToken = default);
    }
}
