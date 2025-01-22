using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolManagementAppV2.Models
{
    public class Tool
    {
        public string ToolID { get; set; }
        public string Name { get; set; }
        public string PartNumber { get; set; }
        public string Description { get; set; }
        public string Brand { get; set; }
        public string Location { get; set; }
        public int QuantityOnHand { get; set; }
        public string Supplier { get; set; }
        public DateTime? PurchasedDate { get; set; }
        public string Notes { get; set; }
        public bool IsCheckedOut { get; set; }
        public string CheckedOutBy { get; set; }
        public DateTime? CheckedOutTime { get; set; }
    }
}
