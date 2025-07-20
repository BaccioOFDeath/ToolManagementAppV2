using System.IO;
using System.Linq;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Interfaces;
using Xunit;

namespace ToolManagementAppV2.Tests.Services
{
    public class ToolCheckOutTests
    {
        [Fact]
        public void ToggleCheckOut_NoQuantity_DoesNothing()
        {
            var db = Path.GetTempFileName();
            try
            {
                var service = new ToolService(new DatabaseService(db));
                service.AddTool(new Tool { ToolNumber = "T1", QuantityOnHand = 0 });
                var tool = service.GetAllTools().First();
                service.ToggleToolCheckOutStatus(tool.ToolID, "u");
                var updated = service.GetToolByID(tool.ToolID);
                Assert.False(updated.IsCheckedOut);
                Assert.Equal(0, updated.QuantityOnHand);
            }
            finally
            {
                if (File.Exists(db)) File.Delete(db);
            }
        }

        [Fact]
        public void ToggleCheckOut_UpdatesQuantity()
        {
            var db = Path.GetTempFileName();
            try
            {
                IToolService svc = new ToolService(new DatabaseService(db));
                svc.AddTool(new Tool { ToolNumber = "T2", QuantityOnHand = 1 });
                var tool = svc.GetAllTools().First();
                svc.ToggleToolCheckOutStatus(tool.ToolID, "u");
                var outTool = svc.GetToolByID(tool.ToolID);
                Assert.True(outTool.IsCheckedOut);
                Assert.Equal(0, outTool.QuantityOnHand);
                svc.ToggleToolCheckOutStatus(tool.ToolID, "u");
                var back = svc.GetToolByID(tool.ToolID);
                Assert.False(back.IsCheckedOut);
                Assert.Equal(1, back.QuantityOnHand);
            }
            finally
            {
                if (File.Exists(db)) File.Delete(db);
            }
        }
    }
}
