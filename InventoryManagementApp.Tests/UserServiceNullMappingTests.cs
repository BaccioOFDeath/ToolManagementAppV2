using System;
using System.Data;
using System.Reflection;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Users;
using Xunit;

public class UserServiceNullMappingTests
{
    [Fact]
    public void MapUser_CoalescesNullStrings()
    {
        var table = new DataTable();
        table.Columns.Add("UserID", typeof(int));
        table.Columns.Add("UserName", typeof(string));
        table.Columns.Add("PasswordHash", typeof(string));
        table.Columns.Add("PasswordSalt", typeof(string));
        table.Columns.Add("UserPhotoPath", typeof(string));
        table.Columns.Add("IsAdmin", typeof(int));
        table.Columns.Add("Email", typeof(string));
        table.Columns.Add("Phone", typeof(string));
        table.Columns.Add("Mobile", typeof(string));
        table.Columns.Add("Address", typeof(string));
        table.Columns.Add("Role", typeof(string));
        table.Columns.Add("IsActive", typeof(int));
        table.Columns.Add("CreatedAt", typeof(DateTime));
        table.Columns.Add("PasswordExpired", typeof(int));

        table.Rows.Add(1, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value,
            0, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value,
            DBNull.Value, 0, DateTime.UtcNow, 0);

        using var reader = table.CreateDataReader();
        reader.Read();

        using var db = new DatabaseService(":memory:");
        var service = new UserService(db, new DummyUserContext());

        var method = typeof(UserService).GetMethod("MapUser", BindingFlags.NonPublic | BindingFlags.Instance);
        var user = (User?)method!.Invoke(service, new object[] { reader });

        Assert.NotNull(user);
        Assert.Equal(string.Empty, user!.UserName);
        Assert.Equal(string.Empty, user.Email);
        Assert.Equal(string.Empty, user.Phone);
        Assert.Equal(string.Empty, user.Address);
    }

    private class DummyUserContext : IUserContext
    {
        public User? CurrentUser { get; set; }
        public event EventHandler<User?>? UserChanged;
        public bool IsAdmin => false;
        public string UserName => CurrentUser?.UserName ?? "Guest";
        public string Role => IsAdmin ? "Admin" : "User";
    }
}

