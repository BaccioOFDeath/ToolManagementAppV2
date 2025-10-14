using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Models.Domain;

namespace InventoryManagementApp.Interfaces
{
    public interface IMonthlyTargetService
    {
        Task<IReadOnlyList<MonthlyTarget>> GetTargetsAsync(int financialYearStart, CancellationToken cancellationToken = default);
        Task SaveTargetsAsync(int financialYearStart, IEnumerable<MonthlyTarget> targets, CancellationToken cancellationToken = default);
    }
}
