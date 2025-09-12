using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DeviceManagementApp.Models;

namespace DeviceManagementApp.Interfaces
{
    public interface IStaffService
    {
        Task<IReadOnlyList<Staff>> GetStaffAsync(CancellationToken cancellationToken = default);
        Task<int> AddStaffAsync(Staff staff, CancellationToken cancellationToken = default);
        Task UpdateStaffAsync(Staff staff, CancellationToken cancellationToken = default);
        Task DeleteStaffAsync(int staffId, CancellationToken cancellationToken = default);
    }
}
