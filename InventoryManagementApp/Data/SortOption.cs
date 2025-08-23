namespace InventoryManagementApp.Data;

public readonly record struct SortOption(SortField Field, SortDirection Direction, string DisplayName);
