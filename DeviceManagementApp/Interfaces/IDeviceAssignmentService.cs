using System.Threading;
using System.Threading.Tasks;
using DeviceManagementApp.Models;

namespace DeviceManagementApp.Interfaces
{
    public interface IDeviceAssignmentService
    {
        Task<DeviceAssignment?> GetCurrentAssignmentAsync(string deviceIp, CancellationToken cancellationToken = default);
        Task<int> AssignDeviceAsync(DeviceAssignment assignment, CancellationToken cancellationToken = default);
        Task ReturnDeviceAsync(string deviceIp, CancellationToken cancellationToken = default);
    }
}
