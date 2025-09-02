using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Models;

namespace InventoryManagementApp.Interfaces
{
    public interface IDeviceGroupService
    {
        Task<IEnumerable<DeviceGroup>> GetGroupsAsync(CancellationToken cancellationToken = default);
        Task<int> CreateGroupAsync(string name, CancellationToken cancellationToken = default);
        Task UpdateGroupAsync(DeviceGroup group, CancellationToken cancellationToken = default);
        Task DeleteGroupAsync(int groupId, CancellationToken cancellationToken = default);
        Task AssignDeviceToGroupAsync(string deviceIp, int? groupId, CancellationToken cancellationToken = default);
        Task<int?> GetDeviceGroupIdAsync(string deviceIp, CancellationToken cancellationToken = default);
    }
}
