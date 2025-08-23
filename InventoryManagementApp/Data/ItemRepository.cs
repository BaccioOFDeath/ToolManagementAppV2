using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Item = InventoryManagementApp.Models.Domain.ItemModel;

namespace InventoryManagementApp.Data;

public sealed class ItemRepository : IItemRepository
{
    private readonly SqliteConnectionFactory _factory;

    public ItemRepository(SqliteConnectionFactory factory)
        => _factory = factory;

    public async IAsyncEnumerable<Item> GetPageAsync(ItemFilter filter, ItemPage page, [EnumeratorCancellation] CancellationToken ct)
    {
        var sql = "SELECT ItemID, ItemNumber, NameDescription AS Name, Location, Brand, PartNumber, Supplier, PurchasedDate, Notes, Keywords, AvailableQuantity AS QuantityOnHand, RentedQuantity, ImagePath, IsCheckedOut, CheckedOutBy, CheckedOutTime, IsPowered FROM Items";
        if (!string.IsNullOrWhiteSpace(filter.Search))
            sql += " WHERE ItemNumber LIKE @Search COLLATE NOCASE OR NameDescription LIKE @Search COLLATE NOCASE";
        sql += " ORDER BY ItemID LIMIT @Take OFFSET @Skip";
        var param = new { Search = $"%{filter.Search}%", Take = page.Size, Skip = (page.Number - 1) * page.Size };
        await using var conn = _factory.Create();
        await using var reader = await conn.ExecuteReaderAsync(
            new CommandDefinition(sql, param, cancellationToken: ct)).ConfigureAwait(false);

        var ordinalItemID = reader.GetOrdinal("ItemID");
        var ordinalItemNumber = reader.GetOrdinal("ItemNumber");
        var ordinalName = reader.GetOrdinal("Name");
        var ordinalLocation = reader.GetOrdinal("Location");
        var ordinalBrand = reader.GetOrdinal("Brand");
        var ordinalPartNumber = reader.GetOrdinal("PartNumber");
        var ordinalSupplier = reader.GetOrdinal("Supplier");
        var ordinalPurchasedDate = reader.GetOrdinal("PurchasedDate");
        var ordinalNotes = reader.GetOrdinal("Notes");
        var ordinalKeywords = reader.GetOrdinal("Keywords");
        var ordinalQuantityOnHand = reader.GetOrdinal("QuantityOnHand");
        var ordinalRentedQuantity = reader.GetOrdinal("RentedQuantity");
        var ordinalImagePath = reader.GetOrdinal("ImagePath");
        var ordinalIsCheckedOut = reader.GetOrdinal("IsCheckedOut");
        var ordinalCheckedOutBy = reader.GetOrdinal("CheckedOutBy");
        var ordinalCheckedOutTime = reader.GetOrdinal("CheckedOutTime");
        var ordinalIsPowered = reader.GetOrdinal("IsPowered");

        while (await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            yield return new Item
            {
                ItemID = !reader.IsDBNull(ordinalItemID) ? reader.GetInt32(ordinalItemID) : 0,
                ItemNumber = reader.IsDBNull(ordinalItemNumber) ? string.Empty : reader.GetString(ordinalItemNumber),
                Name = reader.IsDBNull(ordinalName) ? string.Empty : reader.GetString(ordinalName),
                Location = reader.IsDBNull(ordinalLocation) ? string.Empty : reader.GetString(ordinalLocation),
                Brand = reader.IsDBNull(ordinalBrand) ? string.Empty : reader.GetString(ordinalBrand),
                PartNumber = reader.IsDBNull(ordinalPartNumber) ? string.Empty : reader.GetString(ordinalPartNumber),
                Supplier = reader.IsDBNull(ordinalSupplier) ? string.Empty : reader.GetString(ordinalSupplier),
                PurchasedDate = reader.IsDBNull(ordinalPurchasedDate) ? null : reader.GetDateTime(ordinalPurchasedDate),
                Notes = reader.IsDBNull(ordinalNotes) ? string.Empty : reader.GetString(ordinalNotes),
                Keywords = reader.IsDBNull(ordinalKeywords) ? string.Empty : reader.GetString(ordinalKeywords),
                QuantityOnHand = reader.IsDBNull(ordinalQuantityOnHand) ? 0 : reader.GetInt32(ordinalQuantityOnHand),
                RentedQuantity = reader.IsDBNull(ordinalRentedQuantity) ? 0 : reader.GetInt32(ordinalRentedQuantity),
                ImagePath = reader.IsDBNull(ordinalImagePath) ? string.Empty : reader.GetString(ordinalImagePath),
                IsCheckedOut = !reader.IsDBNull(ordinalIsCheckedOut) && reader.GetInt32(ordinalIsCheckedOut) == 1,
                CheckedOutBy = reader.IsDBNull(ordinalCheckedOutBy) ? string.Empty : reader.GetString(ordinalCheckedOutBy),
                CheckedOutTime = reader.IsDBNull(ordinalCheckedOutTime) ? null : reader.GetDateTime(ordinalCheckedOutTime),
                IsPowered = !reader.IsDBNull(ordinalIsPowered) && reader.GetInt32(ordinalIsPowered) == 1
            };
        }
    }

    public async Task<int> CountAsync(ItemFilter filter, CancellationToken ct)
    {
        var sql = "SELECT COUNT(*) FROM Items";
        object param = new { Search = $"%{filter.Search}%" };
        if (!string.IsNullOrWhiteSpace(filter.Search))
            sql += " WHERE ItemNumber LIKE @Search COLLATE NOCASE OR NameDescription LIKE @Search COLLATE NOCASE";
        using var conn = _factory.Create();
        return await conn.ExecuteScalarAsync<int>(new CommandDefinition(sql, param, cancellationToken: ct));
    }

    public async Task SaveChangesAsync(IEnumerable<Item> changes, CancellationToken ct)
    {
        using var conn = _factory.Create();
        using var tx = conn.BeginTransaction();
        const string sql = "UPDATE Items SET NameDescription=@Name, Location=@Location, AvailableQuantity=@QuantityOnHand WHERE ItemID=@ItemID";
        foreach (var item in changes)
        {
            ct.ThrowIfCancellationRequested();
            await conn.ExecuteAsync(sql, new
            {
                item.Name,
                item.Location,
                QuantityOnHand = item.QuantityOnHand,
                item.ItemID
            }, tx);
        }
        tx.Commit();
    }
}
