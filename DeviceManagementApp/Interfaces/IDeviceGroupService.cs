using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DeviceManagementApp.Models;

namespace DeviceManagementApp.Interfaces
{
    public interface IDeviceGroupService
    {
        Task<IEnumerable<DeviceGroup>> GetGroupsAsync(CancellationToken cancellationToken = default);
        Task<int> CreateGroupAsync(string name, CancellationToken cancellationToken = default);
        Task UpdateGroupAsync(DeviceGroup group, CancellationToken cancellationToken = default);
        Task DeleteGroupAsync(int groupId, CancellationToken cancellationToken = default);
        Task AssignDeviceToGroupAsync(string deviceIp, int? devicePort, int? groupId, CancellationToken cancellationToken = default);
        Task<int?> GetDeviceGroupIdAsync(string deviceIp, int? devicePort, CancellationToken cancellationToken = default);
    }
}
