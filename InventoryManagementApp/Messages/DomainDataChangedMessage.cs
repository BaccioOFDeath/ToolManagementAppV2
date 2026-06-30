using System;

namespace InventoryManagementApp.Messages
{
    [Flags]
    public enum DomainDataScope
    {
        None = 0,
        Items = 1,
        Customers = 2,
        Rentals = 4,
        Reservations = 8,
        Maintenance = 16,
        Calibration = 32,
        Kits = 64,
        Users = 128,
        Categories = 256,
        ActivityLogs = 512,
        Reports = 1024,
        Settings = 2048,
        All = Items | Customers | Rentals | Reservations | Maintenance | Calibration | Kits | Users | Categories | ActivityLogs | Reports | Settings
    }

    public sealed class DomainDataChangedMessage
    {
        public DomainDataChangedMessage(DomainDataScope scope, int? entityId = null)
        {
            Scope = scope;
            EntityId = entityId;
        }

        public DomainDataScope Scope { get; }

        public int? EntityId { get; }

        public bool Includes(DomainDataScope scope) => (Scope & scope) != 0;
    }
}
