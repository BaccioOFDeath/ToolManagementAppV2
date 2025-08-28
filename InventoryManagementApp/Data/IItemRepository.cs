using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Item = InventoryManagementApp.Models.Domain.ItemModel;

namespace InventoryManagementApp.Data;

public interface IItemRepository
{
    /// <summary>
    /// Streams a page of items that match the supplied filter.
    /// </summary>
    IAsyncEnumerable<Item> GetPageAsync(ItemFilter filter, ItemPage page, [EnumeratorCancellation] CancellationToken cancellationToken);
    Task<int> CountAsync(ItemFilter filter, CancellationToken cancellationToken);
    Task<Item?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task SaveChangesAsync(IEnumerable<Item> changes, CancellationToken cancellationToken);
    Task<int> InsertAsync(Item item, CancellationToken cancellationToken);
    Task UpdateAsync(Item item, CancellationToken cancellationToken);
    Task DeleteAsync(int itemID, CancellationToken cancellationToken);
    Task<bool> ToggleCheckOutStatusAsync(int itemID, string currentUser, bool isAdmin, CancellationToken cancellationToken);
    Task<List<Item>> GetItemsCheckedOutByAsync(string userName, CancellationToken cancellationToken);
    Task<List<Item>> GetCheckedOutItemsAsync(CancellationToken cancellationToken);
    Task UpdateItemImageAsync(int itemID, string imagePath, CancellationToken cancellationToken);
}
