namespace InventoryManagementApp.Models.DTOs
{
    public class VehiclePipelineSummary
    {
        public int Received { get; set; }
        public int OnHold { get; set; }
        public int Dismantling { get; set; }
        public int Completed { get; set; }
    }
}
