using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolManagementAppV2.Models
{
    public class Rental
    {
        public int RentalID { get; set; }
        public int ToolID { get; set; }
        public int CustomerID { get; set; }
        public DateTime RentalDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; } // Nullable to indicate if returned or not
        public string? Status { get; set; }
    }
}
