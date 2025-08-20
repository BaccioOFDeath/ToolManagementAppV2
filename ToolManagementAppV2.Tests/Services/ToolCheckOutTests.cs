using System;
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
                var service = new ItemService(new DatabaseService(db));
                service.AddTool(new ItemModel { ItemNumber = "T1", QuantityOnHand = 0 });
                var tool = service.GetAllTools().First();
                var result = service.ToggleToolCheckOutStatus(tool.ItemID, "u");
                var updated = service.GetToolByID(tool.ItemID);
                Assert.False(result);
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
                IItemService svc = new ItemService(new DatabaseService(db));
                svc.AddTool(new ItemModel { ItemNumber = "T2", QuantityOnHand = 1 });
                var tool = svc.GetAllTools().First();
                var first = svc.ToggleToolCheckOutStatus(tool.ItemID, "u");
                var outTool = svc.GetToolByID(tool.ItemID);
                Assert.True(first);
                Assert.True(outTool.IsCheckedOut);
                Assert.Equal(0, outTool.QuantityOnHand);
                var second = svc.ToggleToolCheckOutStatus(tool.ItemID, "u");
                var back = svc.GetToolByID(tool.ItemID);
                Assert.True(second);
                Assert.False(back.IsCheckedOut);
                Assert.Equal(1, back.QuantityOnHand);
            }
            finally
            {
                if (File.Exists(db)) File.Delete(db);
            }
        }

        [Fact]
        public void ToggleCheckOut_Nonexistent_Throws()
        {
            var db = Path.GetTempFileName();
            try
            {
                var service = new ItemService(new DatabaseService(db));
                Assert.Throws<InvalidOperationException>(() => service.ToggleToolCheckOutStatus(42, "u"));
            }
            finally
            {
                if (File.Exists(db)) File.Delete(db);
            }
        }
    }
}
