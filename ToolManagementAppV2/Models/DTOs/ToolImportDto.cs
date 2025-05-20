namespace ToolManagementAppV2.Models.ImportExport
{
    public class ToolImportDto
    {
        public string ToolNumber { get; set; }
        public string NameDescription { get; set; }
        public string Location { get; set; }
        public string Brand { get; set; }
        public string PartNumber { get; set; }
        public string Supplier { get; set; }
        public DateTime? PurchasedDate { get; set; }
        public string Notes { get; set; }
        public string Keywords { get; set; }
        public int AvailableQuantity { get; set; }
        public int RentedQuantity { get; set; }
    }
}
