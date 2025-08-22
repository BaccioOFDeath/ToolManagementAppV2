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
        var sql = "SELECT * FROM Items";
        if (!string.IsNullOrWhiteSpace(filter.Search))
            sql += " WHERE ItemNumber LIKE @Search OR NameDescription LIKE @Search";
        sql += " ORDER BY ItemID LIMIT @Take OFFSET @Skip";
        var param = new { Search = $"%{filter.Search}%", Take = page.Size, Skip = (page.Number - 1) * page.Size };
        using var conn = _factory.Create();
        var rows = await conn.QueryAsync<Item>(
            new CommandDefinition(sql, param, flags: CommandFlags.None, cancellationToken: ct));
        foreach (var row in rows)
        {
            ct.ThrowIfCancellationRequested();
            yield return row;
        }
    }

    public async Task<int> CountAsync(ItemFilter filter, CancellationToken ct)
    {
        var sql = "SELECT COUNT(*) FROM Items";
        object param = new { Search = $"%{filter.Search}%" };
        if (!string.IsNullOrWhiteSpace(filter.Search))
            sql += " WHERE ItemNumber LIKE @Search OR NameDescription LIKE @Search";
        using var conn = _factory.Create();
        return await conn.ExecuteScalarAsync<int>(new CommandDefinition(sql, param, cancellationToken: ct));
    }

    public async Task SaveChangesAsync(IEnumerable<Item> changes, CancellationToken ct)
    {
        using var conn = _factory.Create();
        using var tx = conn.BeginTransaction();
        const string sql = "UPDATE Items SET NameDescription=@NameDescription, Location=@Location, AvailableQuantity=@QuantityOnHand WHERE ItemID=@ItemID";
        foreach (var item in changes)
        {
            ct.ThrowIfCancellationRequested();
            await conn.ExecuteAsync(sql, new
            {
                item.NameDescription,
                item.Location,
                QuantityOnHand = item.QuantityOnHand,
                item.ItemID
            }, tx);
        }
        tx.Commit();
    }
}
