using System;
using System.IO;
using System.Linq;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Items;
using InventoryManagementApp.Interfaces;
using Xunit;

namespace InventoryManagementApp.Tests.Services
{
    public class ItemCheckOutTests
    {
        [Fact]
        public void ToggleCheckOut_NoQuantity_DoesNothing()
        {
            var db = Path.GetTempFileName();
            try
            {
                var service = new ItemService(new DatabaseService(db));
                service.AddItem(new ItemModel { ItemNumber = "T1", QuantityOnHand = 0 });
                var item = service.GetAllItems().First();
                var result = service.ToggleItemCheckOutStatus(item.ItemID, "u");
                var updated = service.GetItemByID(item.ItemID);
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
                var item = svc.GetAllItems().First();
                var first = svc.ToggleItemCheckOutStatus(item.ItemID, "u");
                var outItem = svc.GetItemByID(item.ItemID);
                Assert.True(first);
                Assert.True(outItem.IsCheckedOut);
                Assert.Equal(0, outItem.QuantityOnHand);
                var second = svc.ToggleItemCheckOutStatus(item.ItemID, "u");
                var back = svc.GetItemByID(item.ItemID);
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
