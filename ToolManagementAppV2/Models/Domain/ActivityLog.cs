﻿namespace ToolManagementAppV2.Models.Domain
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
