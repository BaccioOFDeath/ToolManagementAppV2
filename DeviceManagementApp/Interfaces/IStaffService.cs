using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DeviceManagementApp.Interfaces
{
    public interface IStaffService
    {
        Task<IEnumerable<KeyValuePair<int, string>>> GetStaffAsync(CancellationToken cancellationToken = default);
    }
}
