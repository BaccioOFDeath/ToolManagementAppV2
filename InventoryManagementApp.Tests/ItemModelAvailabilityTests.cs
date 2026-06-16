using System;
using InventoryManagementApp.Models.Domain;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ItemModelAvailabilityTests
    {
        [Fact]
        public void AvailableItem_ShowsOnHandLocationDetail()
        {
            var item = new ItemModel
            {
                QuantityOnHand = 3,
                Location = "Bay 4"
            };

            Assert.False(item.IsUnavailable);
            Assert.Equal("Available", item.AvailabilityStatus);
            Assert.Equal("3 on hand at Bay 4.", item.AvailabilityDetail);
            Assert.Equal("On hand: 3 | Rented: 0", item.StockSummary);
        }

        [Fact]
        public void CheckedOutItem_ShowsHolderAndOutSinceDetail()
        {
            var item = new ItemModel
            {
                QuantityOnHand = 0,
                IsCheckedOut = true,
                CheckedOutBy = "Alex Technician",
                CheckedOutTime = new DateTime(2026, 6, 16, 9, 30, 0)
            };

            Assert.True(item.IsUnavailable);
            Assert.Equal("Checked Out", item.AvailabilityStatus);
            Assert.Equal("Alex Technician", item.HolderDisplay);
            Assert.Equal("Out to Alex Technician since 2026-06-16 09:30", item.AvailabilityDetail);
            Assert.Equal("Out since 2026-06-16 09:30", item.ActivitySummary);
        }

        [Fact]
        public void IncompleteItem_TakesPriorityOverOtherUnavailableStates()
        {
            var item = new ItemModel
            {
                QuantityOnHand = 0,
                RentedQuantity = 2,
                IsCheckedOut = true,
                IsIncomplete = true,
                MissingComponentsNotes = "Battery missing"
            };

            Assert.True(item.IsUnavailable);
            Assert.Equal("Incomplete", item.AvailabilityStatus);
            Assert.Equal("Battery missing", item.AvailabilityDetail);
        }
    }
}
