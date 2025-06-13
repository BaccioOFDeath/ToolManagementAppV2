namespace ToolManagementAppV2.Models.Domain
{
    public class Tool
    {
        public string ToolID { get; set; } = string.Empty;
        public string ToolNumber { get; set; } = string.Empty;
        public string PartNumber { get; set; } = string.Empty;
        public string NameDescription { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int QuantityOnHand { get; set; }
        public int RentedQuantity { get; set; }
        public string Supplier { get; set; } = string.Empty;
        public DateTime? PurchasedDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string Keywords { get; set; } = string.Empty;
        public bool IsCheckedOut { get; set; }
        public string CheckedOutBy { get; set; } = string.Empty;
        public DateTime? CheckedOutTime { get; set; }
        public string ToolImagePath { get; set; } = string.Empty;

        public int OnHand => QuantityOnHand;
        public string Purchased => PurchasedDate?.ToString("yyyy-MM-dd") ?? "";
    }
}
