using System.Collections.Generic;

namespace InventoryManagementApp.Models
{
    public class DiscoveredDevice
    {
        public string Ip { get; set; } = string.Empty;
        public string Hostname { get; set; } = string.Empty;
        public string MacAddress { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public IList<DeviceProtocol> Protocols { get; set; } = new List<DeviceProtocol>();
    }
}
