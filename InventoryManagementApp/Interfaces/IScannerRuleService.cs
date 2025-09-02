using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Models;

namespace InventoryManagementApp.Interfaces
{
    public interface IScannerRuleService
    {
        Task<int> AddRuleAsync(ScannerFileRule rule, CancellationToken cancellationToken = default);
        Task<IEnumerable<ScannerFileRule>> GetRulesAsync(string deviceId, CancellationToken cancellationToken = default);
        Task DeleteRuleAsync(int ruleId, CancellationToken cancellationToken = default);
    }
}
