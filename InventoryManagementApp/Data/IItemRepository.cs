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
    Task<int> InsertAsync(Item item, CancellationToken ct);
    Task UpdateAsync(Item item, CancellationToken ct);
    Task DeleteAsync(int itemID, CancellationToken ct);
    Task<bool> ToggleCheckOutStatusAsync(int itemID, string currentUser, bool isAdmin, CancellationToken ct);
    Task<List<Item>> GetItemsCheckedOutByAsync(string userName, CancellationToken ct);
    Task<List<Item>> GetCheckedOutItemsAsync(CancellationToken ct);
    Task UpdateItemImageAsync(int itemID, string imagePath, CancellationToken ct);
}
