namespace InventoryManagementApp.Models.ImportExport
{
    public class CustomerImportResult
    {
        public int ImportedCount { get; set; }
        public List<string> SkippedRows { get; } = new();
    }
}
