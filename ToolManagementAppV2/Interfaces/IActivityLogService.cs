using System;
using System.Collections.Generic;
using ToolManagementAppV2.Models;
using ToolManagementAppV2.Models.Domain;

namespace ToolManagementAppV2.Interfaces
{
    internal interface IActivityLogService
    {
        void LogAction(int userID, string userName, string action);
        List<ActivityLog> GetRecentLogs(int count = 50);
        void PurgeOldLogs(DateTime threshold);
    }
}
