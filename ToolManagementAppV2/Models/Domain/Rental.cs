namespace ToolManagementAppV2.Models.Domain
{
    public class Rental
    {
        public int RentalID { get; set; }
        // Stored as INTEGER in the database but exposed as a string for
        // consistency with ToolModel.ToolID and service signatures.
        public string ToolID { get; set; }
        public int CustomerID { get; set; }
        public DateTime RentalDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; } // Nullable to indicate if returned or not
        public string? Status { get; set; }
    }
}
