using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Models;

namespace InventoryManagementApp.Interfaces
{
    public interface IEmailAccountDiscoveryService
    {
        Task<IReadOnlyList<EmailAccountOption>> GetOutlookAccountsAsync(CancellationToken cancellationToken = default);
    }
}
