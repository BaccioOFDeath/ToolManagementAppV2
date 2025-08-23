namespace InventoryManagementApp.Models.ImportExport
{
    public class ItemImportDto
    {
        public string? ItemNumber { get; set; }
        public string? Name { get; set; }
        public string? Location { get; set; }
        public string? Brand { get; set; }
        public string? PartNumber { get; set; }
        public string? Supplier { get; set; }
        public DateTime? PurchasedDate { get; set; }
        public string? Notes { get; set; }
        public string? Keywords { get; set; }
        public int AvailableQuantity { get; set; }
        public int RentedQuantity { get; set; }
        public bool IsRentalItem { get; set; }
    }
}
