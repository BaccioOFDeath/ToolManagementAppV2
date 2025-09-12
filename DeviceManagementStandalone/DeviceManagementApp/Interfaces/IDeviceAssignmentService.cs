using System.Threading;
using System.Threading.Tasks;
using DeviceManagementApp.Models;

namespace DeviceManagementApp.Interfaces
{
    public interface IDeviceAssignmentService
    {
        Task<DeviceAssignment?> GetCurrentAssignmentAsync(string deviceIp, CancellationToken cancellationToken = default);
        Task AssignAsync(DeviceAssignment assignment, CancellationToken cancellationToken = default);
        Task ReturnAsync(string deviceIp, CancellationToken cancellationToken = default);
    }
}
