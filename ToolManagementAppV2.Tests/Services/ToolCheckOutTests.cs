using System;
using System.IO;
using System.Linq;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Items;
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
                service.AddItem(new ItemModel { ItemNumber = "T1", QuantityOnHand = 0 });
                var tool = service.GetAllItems().First();
                var result = service.ToggleItemCheckOutStatus(tool.ItemID, "u");
                var updated = service.GetItemByID(tool.ItemID);
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
                svc.AddItem(new ItemModel { ItemNumber = "T2", QuantityOnHand = 1 });
                var tool = svc.GetAllItems().First();
                var first = svc.ToggleItemCheckOutStatus(tool.ItemID, "u");
                var outTool = svc.GetItemByID(tool.ItemID);
                Assert.True(first);
                Assert.True(outTool.IsCheckedOut);
                Assert.Equal(0, outTool.QuantityOnHand);
                var second = svc.ToggleItemCheckOutStatus(tool.ItemID, "u");
                var back = svc.GetItemByID(tool.ItemID);
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
                Assert.Throws<InvalidOperationException>(() => service.ToggleItemCheckOutStatus(42, "u"));
            }
            finally
            {
                if (File.Exists(db)) File.Delete(db);
            }
        }
    }
}
