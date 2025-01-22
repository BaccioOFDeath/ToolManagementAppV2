using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolManagementAppV2.Models
{
    public class ActivityLog
    {
        public int LogID { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string Action { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
