using System;

namespace DeviceManagementApp.Models
{
    public class DeviceAssignment
    {
        public int AssignmentId { get; set; }
        public string DeviceIp { get; set; } = string.Empty;
        public int UserId { get; set; }
        public DateTime AssignedDate { get; set; }
        public DateTime? ReturnedDate { get; set; }
        public int? DepartmentId { get; set; }
    }
}
