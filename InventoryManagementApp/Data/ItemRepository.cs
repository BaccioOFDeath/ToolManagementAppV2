using System;
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
        ct.ThrowIfCancellationRequested();
        var sql = "SELECT ItemID, ItemNumber, NameDescription AS Name, Location, Brand, PartNumber, Supplier, PurchasedDate, Notes, Keywords, AvailableQuantity AS QuantityOnHand, RentedQuantity, IsRentalItem, Price, ImagePath, IsCheckedOut, CheckedOutBy, CheckedOutTime, CheckedInBy, CheckedInTime, IsPowered, UpdatedAt FROM Items";
        var (whereClause, parameters) = BuildFilter(filter);
        sql += whereClause;
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
        var command = new CommandDefinition(sql, parameters, flags: CommandFlags.None, cancellationToken: ct);
        await using var reader = await conn.ExecuteReaderAsync(command).ConfigureAwait(false);
        var parser = reader.GetRowParser<Item>();
        while (await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            yield return parser(reader);
        }
    }

    public async Task<int> CountAsync(ItemFilter filter, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var sql = "SELECT COUNT(*) FROM Items";
        var (whereClause, parameters) = BuildFilter(filter);
        sql += whereClause;
        await using var conn = (DbConnection)_factory.Create();
        return await conn.ExecuteScalarAsync<int>(new CommandDefinition(sql, parameters, cancellationToken: ct));
    }

    public async Task<Item?> GetByIdAsync(int id, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        const string sql = @"SELECT ItemID, ItemNumber, NameDescription AS Name, Location, Brand, PartNumber, Supplier,
            PurchasedDate, Notes, Keywords, AvailableQuantity AS QuantityOnHand, RentedQuantity, IsRentalItem, Price, ImagePath,
            IsCheckedOut, CheckedOutBy, CheckedOutTime, CheckedInBy, CheckedInTime, IsPowered, UpdatedAt FROM Items WHERE ItemID=@ID";
        await using var conn = (DbConnection)_factory.Create();
        return await conn.QueryFirstOrDefaultAsync<Item>(new CommandDefinition(sql, new { ID = id }, cancellationToken: ct)).ConfigureAwait(false);
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

    public async Task<int> InsertAsync(Item item, CancellationToken ct)
    {
        const string sql = @"INSERT INTO Items (ItemNumber, NameDescription, Location, Brand, PartNumber, Supplier, PurchasedDate, Notes, Keywords, AvailableQuantity, RentedQuantity, IsRentalItem, Price, ImagePath, IsCheckedOut, IsPowered)
                             VALUES (@ItemNumber,@Name,@Location,@Brand,@PartNumber,@Supplier,@PurchasedDate,@Notes,@Keywords,@QuantityOnHand,@RentedQuantity,@IsRentalItem,@Price,@ImagePath,0,@IsPowered);
                             SELECT last_insert_rowid();";
        await using var conn = (DbConnection)_factory.Create();
        var id = await conn.ExecuteScalarAsync<long>(new CommandDefinition(sql, new
        {
            item.ItemNumber,
            Name = item.Name,
            item.Location,
            item.Brand,
            PartNumber = item.PartNumber,
            item.Supplier,
            item.PurchasedDate,
            item.Notes,
            item.Keywords,
            QuantityOnHand = item.QuantityOnHand,
            item.RentedQuantity,
            IsRentalItem = item.IsRentalItem ? 1 : 0,
            item.Price,
            item.ImagePath,
            IsPowered = item.IsPowered ? 1 : 0
        }, cancellationToken: ct));
        return (int)id;
    }

    public async Task UpdateAsync(Item item, CancellationToken ct)
    {
        const string sql = @"UPDATE Items SET
                  ItemNumber = @ItemNumber,
                  NameDescription = @Name,
                  Location = @Location,
                  Brand = @Brand,
                  PartNumber = @PartNumber,
                  Supplier = @Supplier,
                  PurchasedDate = @PurchasedDate,
                  Notes = @Notes,
                  Keywords = @Keywords,
                  AvailableQuantity = @QuantityOnHand,
                  RentedQuantity = @RentedQuantity,
                  IsRentalItem = @IsRentalItem,
                  IsPowered = @IsPowered,
                  IsCheckedOut = @IsCheckedOut,
                  CheckedOutBy = @CheckedOutBy,
                  CheckedOutTime = @CheckedOutTime,
                  CheckedInBy = @CheckedInBy,
                  CheckedInTime = @CheckedInTime,
                  ImagePath = @ImagePath
                WHERE ItemID = @ItemID";
        await using var conn = (DbConnection)_factory.Create();
        var rows = await conn.ExecuteAsync(new CommandDefinition(sql, new
        {
            item.ItemNumber,
            Name = item.Name,
            item.Location,
            item.Brand,
            PartNumber = item.PartNumber,
            item.Supplier,
            item.PurchasedDate,
            item.Notes,
            item.Keywords,
            QuantityOnHand = item.QuantityOnHand,
            item.RentedQuantity,
            IsRentalItem = item.IsRentalItem ? 1 : 0,
            IsPowered = item.IsPowered ? 1 : 0,
            IsCheckedOut = item.IsCheckedOut ? 1 : 0,
            item.CheckedOutBy,
            item.CheckedOutTime,
            item.CheckedInBy,
            item.CheckedInTime,
            item.ImagePath,
            item.ItemID
        }, cancellationToken: ct));
        if (rows == 0)
            throw new InvalidOperationException($"Failed to update item {item.ItemID}.");
    }

    public async Task DeleteAsync(int itemID, CancellationToken ct)
    {
        await using var conn = (DbConnection)_factory.Create();
        var rows = await conn.ExecuteAsync(new CommandDefinition("DELETE FROM Items WHERE ItemID=@ID", new { ID = itemID }, cancellationToken: ct));
        if (rows == 0)
            throw new InvalidOperationException($"Failed to delete item {itemID}.");
    }

    public async Task<bool> ToggleCheckOutStatusAsync(int itemID, string currentUser, bool isAdmin, CancellationToken ct)
    {
        await using var conn = (DbConnection)_factory.Create();
        var record = await conn.QueryFirstOrDefaultAsync<(bool Rental, bool Out, int Qty, string? By)>(new CommandDefinition(
            "SELECT IsRentalItem as Rental, IsCheckedOut as Out, AvailableQuantity as Qty, CheckedOutBy as By FROM Items WHERE ItemID=@ID",
            new { ID = itemID }, cancellationToken: ct));

        if (record.Equals(default((bool, bool, int, string?))))
            throw new InvalidOperationException($"Item {itemID} not found.");

        if (record.Rental)
            return false;

        if (!record.Out)
        {
            if (record.Qty <= 0)
                return false;
        }
        else if (!isAdmin && !string.Equals(record.By, currentUser, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var newStatus = record.Out ? 0 : 1;
        var outTime = record.Out ? (DateTime?)null : DateTime.UtcNow;
        var outBy = record.Out ? null : currentUser;
        var inTime = record.Out ? DateTime.UtcNow : (DateTime?)null;
        var inBy = record.Out ? currentUser : null;
        var qtyChange = record.Out ? 1 : -1;

        var rows = await conn.ExecuteAsync(new CommandDefinition(@"UPDATE Items SET
                  IsCheckedOut = @Out,
                  CheckedOutBy = @By,
                  CheckedOutTime = @Time,
                  CheckedInBy = @InBy,
                  CheckedInTime = @InTime,
                  AvailableQuantity = AvailableQuantity + @Q
                WHERE ItemID = @ID",
            new { Out = newStatus, By = outBy, Time = outTime, InBy = inBy, InTime = inTime, Q = qtyChange, ID = itemID }, cancellationToken: ct));

        if (rows == 0)
            throw new InvalidOperationException("Check-out status update failed.");

        return true;
    }

    public async Task<List<Item>> GetItemsCheckedOutByAsync(string userName, CancellationToken ct)
    {
        const string sql = @"SELECT ItemID, ItemNumber, NameDescription AS Name, Location, Brand, PartNumber, Supplier, PurchasedDate, Notes, Keywords, AvailableQuantity AS QuantityOnHand, RentedQuantity, IsRentalItem, Price, ImagePath, IsCheckedOut, CheckedOutBy, CheckedOutTime, CheckedInBy, CheckedInTime, IsPowered, UpdatedAt FROM Items WHERE CheckedOutBy=@User AND IsCheckedOut=1 AND IFNULL(IsRentalItem,0)=0";
        await using var conn = (DbConnection)_factory.Create();
        var items = await conn.QueryAsync<Item>(new CommandDefinition(sql, new { User = userName }, cancellationToken: ct)).ConfigureAwait(false);
        return items.AsList();
    }

    public async Task<List<Item>> GetCheckedOutItemsAsync(CancellationToken ct)
    {
        const string sql = @"SELECT ItemID, ItemNumber, NameDescription AS Name, Location, Brand, PartNumber, Supplier, PurchasedDate, Notes, Keywords, AvailableQuantity AS QuantityOnHand, RentedQuantity, IsRentalItem, Price, ImagePath, IsCheckedOut, CheckedOutBy, CheckedOutTime, CheckedInBy, CheckedInTime, IsPowered, UpdatedAt FROM Items WHERE IsCheckedOut=1 AND IFNULL(IsRentalItem,0)=0";
        await using var conn = (DbConnection)_factory.Create();
        var items = await conn.QueryAsync<Item>(new CommandDefinition(sql, cancellationToken: ct)).ConfigureAwait(false);
        return items.AsList();
    }

    public async Task UpdateItemImageAsync(int itemID, string imagePath, CancellationToken ct)
    {
        await using var conn = (DbConnection)_factory.Create();
        var rows = await conn.ExecuteAsync(new CommandDefinition("UPDATE Items SET ImagePath=@Img WHERE ItemID=@ID", new { Img = imagePath, ID = itemID }, cancellationToken: ct));
        if (rows == 0)
            throw new InvalidOperationException($"Failed to update image for item {itemID}.");
    }

    private static (string WhereClause, DynamicParameters Parameters) BuildFilter(ItemFilter filter)
    {
        var parameters = new DynamicParameters();
        var conditions = new List<string>();
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var tokens = filter.Search.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < tokens.Length; i++)
            {
                conditions.Add("(ItemNumber LIKE @ItemNumberPrefix" + i + " COLLATE NOCASE_NOACCENT OR ItemNumber LIKE @ItemNumberSubstring" + i + " COLLATE NOCASE_NOACCENT OR NameDescription LIKE @NameSubstring" + i + " COLLATE NOCASE_NOACCENT OR Notes LIKE @NotesSubstring" + i + " COLLATE NOCASE_NOACCENT OR Keywords LIKE @KeywordsSubstring" + i + " COLLATE NOCASE_NOACCENT)");
                parameters.Add("ItemNumberPrefix" + i, tokens[i] + "%");
                parameters.Add("ItemNumberSubstring" + i, "%" + tokens[i] + "%");
                parameters.Add("NameSubstring" + i, "%" + tokens[i] + "%");
                parameters.Add("NotesSubstring" + i, "%" + tokens[i] + "%");
                parameters.Add("KeywordsSubstring" + i, "%" + tokens[i] + "%");
            }
        }
        if (filter.IsRentalItem.HasValue)
        {
            conditions.Add("IsRentalItem=@IsRental");
            parameters.Add("IsRental", filter.IsRentalItem.Value ? 1 : 0);
        }
        var whereClause = conditions.Count > 0 ? " WHERE " + string.Join(" AND ", conditions) : string.Empty;
        return (whereClause, parameters);
    }
}
