using System.Collections.Generic;
using ToolManagementAppV2.Models;

namespace ToolManagementAppV2.Interfaces
{
    public interface IScannerService
    {
        IEnumerable<ScannerDevice> GetScannerDevices();
    }
}
