using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Item = InventoryManagementApp.Models.Domain.ItemModel;

namespace InventoryManagementApp.Data;

public interface IItemRepository
{
    IAsyncEnumerable<Item> GetPageAsync(ItemFilter filter, ItemPage page, CancellationToken ct);
    Task<int> CountAsync(ItemFilter filter, CancellationToken ct);
    Task SaveChangesAsync(IEnumerable<Item> changes, CancellationToken ct);
}
