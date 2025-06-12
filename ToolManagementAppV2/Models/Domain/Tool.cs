namespace ToolManagementAppV2.Models.Domain
{
    public class Tool
    {
        // ToolID is stored as an INTEGER PRIMARY KEY in the database.
        // It is represented as a string in the model for easier binding and
        // because most services treat it as a string key.
        public string ToolID { get; set; } // hide in UI, used internally
        public string ToolNumber { get; set; }
        public string PartNumber { get; set; }
        public string NameDescription { get; set; }
        public string Brand { get; set; }
        public string Location { get; set; }
        public int QuantityOnHand { get; set; }
        public int RentedQuantity { get; set; }
        public string Supplier { get; set; }
        public DateTime? PurchasedDate { get; set; }
        public string Notes { get; set; }
        public string Keywords { get; set; }
        public bool IsCheckedOut { get; set; }
        public string CheckedOutBy { get; set; }
        public DateTime? CheckedOutTime { get; set; }
        public string ToolImagePath { get; set; }

        public int OnHand => QuantityOnHand;
        public string Purchased => PurchasedDate?.ToString("yyyy-MM-dd") ?? "";
    }
}