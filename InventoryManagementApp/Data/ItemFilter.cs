namespace InventoryManagementApp.Data;

public enum SortField
{
    Name,
    ItemNumber,
    QuantityOnHand,
    UpdatedAt
}

public enum SortDirection
{
    Ascending,
    Descending
}

public readonly record struct ItemFilter(string? Search, SortField SortField = SortField.Name, SortDirection SortDirection = SortDirection.Ascending);
