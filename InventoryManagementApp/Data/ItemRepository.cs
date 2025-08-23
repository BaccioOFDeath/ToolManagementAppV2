using System.Collections.Generic;
using System.Data.Common;
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
        var sql = "SELECT ItemID, ItemNumber, NameDescription AS Name, Location, Brand, PartNumber, Supplier, PurchasedDate, Notes, Keywords, AvailableQuantity AS QuantityOnHand, RentedQuantity, IsRentalItem, Price, ImagePath, IsCheckedOut, CheckedOutBy, CheckedOutTime, CheckedInBy, CheckedInTime, IsPowered, UpdatedAt FROM Items";
        var parameters = new DynamicParameters();
        var conditions = new List<string>();
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            conditions.Add("(ItemNumber LIKE @ItemNumberPrefix COLLATE NOCASE_NOACCENT OR ItemNumber LIKE @ItemNumberSubstring COLLATE NOCASE_NOACCENT OR NameDescription LIKE @NameSubstring COLLATE NOCASE_NOACCENT)");
            parameters.Add("ItemNumberPrefix", $"{filter.Search}%");
            parameters.Add("ItemNumberSubstring", $"%{filter.Search}%");
            parameters.Add("NameSubstring", $"%{filter.Search}%");
        }
        if (filter.IsRentalItem.HasValue)
        {
            conditions.Add("IsRentalItem=@IsRental");
            parameters.Add("IsRental", filter.IsRentalItem.Value ? 1 : 0);
        }
        if (conditions.Count > 0)
            sql += " WHERE " + string.Join(" AND ", conditions);
        var orderColumn = filter.SortField switch
        {
            SortField.Name => "NameDescription",
            SortField.ItemNumber => "ItemNumber",
            SortField.QuantityOnHand => "AvailableQuantity",
            SortField.UpdatedAt => "UpdatedAt",
            _ => "ItemID"
        };
        var orderDirection = filter.SortDirection == SortDirection.Ascending ? "ASC" : "DESC";
        sql += $" ORDER BY {orderColumn} {orderDirection}, ItemID ASC LIMIT @Take OFFSET @Skip";
        parameters.Add("Take", page.Size);
        parameters.Add("Skip", (page.Number - 1) * page.Size);
        await using var conn = (DbConnection)_factory.Create();
        var command = new CommandDefinition(sql, parameters, flags: CommandFlags.Pipelined, cancellationToken: ct);
        await using var reader = await conn.ExecuteReaderAsync(command).ConfigureAwait(false);

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
        var ordinalIsRental = reader.GetOrdinal("IsRentalItem");
        var ordinalPrice = reader.GetOrdinal("Price");
        var ordinalImagePath = reader.GetOrdinal("ImagePath");
        var ordinalIsCheckedOut = reader.GetOrdinal("IsCheckedOut");
        var ordinalCheckedOutBy = reader.GetOrdinal("CheckedOutBy");
        var ordinalCheckedOutTime = reader.GetOrdinal("CheckedOutTime");
        var ordinalCheckedInBy = reader.GetOrdinal("CheckedInBy");
        var ordinalCheckedInTime = reader.GetOrdinal("CheckedInTime");
        var ordinalIsPowered = reader.GetOrdinal("IsPowered");
        var ordinalUpdatedAt = reader.GetOrdinal("UpdatedAt");

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
                IsRentalItem = !reader.IsDBNull(ordinalIsRental) && reader.GetInt32(ordinalIsRental) == 1,
                Price = reader.IsDBNull(ordinalPrice) ? 0m : reader.GetDecimal(ordinalPrice),
                ImagePath = reader.IsDBNull(ordinalImagePath) ? string.Empty : reader.GetString(ordinalImagePath),
                IsCheckedOut = !reader.IsDBNull(ordinalIsCheckedOut) && reader.GetInt32(ordinalIsCheckedOut) == 1,
                CheckedOutBy = reader.IsDBNull(ordinalCheckedOutBy) ? string.Empty : reader.GetString(ordinalCheckedOutBy),
                CheckedOutTime = reader.IsDBNull(ordinalCheckedOutTime) ? null : reader.GetDateTime(ordinalCheckedOutTime),
                CheckedInBy = reader.IsDBNull(ordinalCheckedInBy) ? string.Empty : reader.GetString(ordinalCheckedInBy),
                CheckedInTime = reader.IsDBNull(ordinalCheckedInTime) ? null : reader.GetDateTime(ordinalCheckedInTime),
                IsPowered = !reader.IsDBNull(ordinalIsPowered) && reader.GetInt32(ordinalIsPowered) == 1,
                UpdatedAt = reader.IsDBNull(ordinalUpdatedAt) ? default : reader.GetDateTime(ordinalUpdatedAt)
            };
        }
    }

    public async Task<int> CountAsync(ItemFilter filter, CancellationToken ct)
    {
        var sql = "SELECT COUNT(*) FROM Items";
        var parameters = new DynamicParameters();
        var conditions = new List<string>();
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            conditions.Add("(ItemNumber LIKE @ItemNumberPrefix COLLATE NOCASE_NOACCENT OR ItemNumber LIKE @ItemNumberSubstring COLLATE NOCASE_NOACCENT OR NameDescription LIKE @NameSubstring COLLATE NOCASE_NOACCENT)");
            parameters.Add("ItemNumberPrefix", $"{filter.Search}%");
            parameters.Add("ItemNumberSubstring", $"%{filter.Search}%");
            parameters.Add("NameSubstring", $"%{filter.Search}%");
        }
        if (filter.IsRentalItem.HasValue)
        {
            conditions.Add("IsRentalItem=@IsRental");
            parameters.Add("IsRental", filter.IsRentalItem.Value ? 1 : 0);
        }
        if (conditions.Count > 0)
            sql += " WHERE " + string.Join(" AND ", conditions);
        await using var conn = (DbConnection)_factory.Create();
        return await conn.ExecuteScalarAsync<int>(new CommandDefinition(sql, parameters, flags: CommandFlags.Pipelined, cancellationToken: ct));
    }

    public async Task SaveChangesAsync(IEnumerable<Item> changes, CancellationToken ct)
    {
        await using var conn = (DbConnection)_factory.Create();
        using var tx = conn.BeginTransaction();
        const string sql = "UPDATE Items SET NameDescription=@Name, Location=@Location, AvailableQuantity=@QuantityOnHand, Price=@Price WHERE ItemID=@ItemID";
        foreach (var item in changes)
        {
            ct.ThrowIfCancellationRequested();
            await conn.ExecuteAsync(sql, new
            {
                item.Name,
                item.Location,
                QuantityOnHand = item.QuantityOnHand,
                item.Price,
                item.ItemID
            }, tx);
        }
        tx.Commit();
    }
}
