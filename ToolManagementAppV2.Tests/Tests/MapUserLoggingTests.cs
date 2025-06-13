using System;
using System.IO;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Users;
using Xunit;

namespace ToolManagementAppV2.Tests
{
    public class MapUserLoggingTests
    {
        [Fact]
        public void GetAllUsers_InvalidPackPath_LogsException()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var service = new UserService(db);
                service.AddUser(new User { UserName = "u", Password = "p", UserPhotoPath = "pack://application:,,,/Resources/NoImage.png" });

                var sw = new StringWriter();
                var original = Console.Out;
                Console.SetOut(sw);
                try
                {
                    service.GetAllUsers();
                }
                finally
                {
                    Console.SetOut(original);
                }
                Assert.NotEqual(string.Empty, sw.ToString());
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public void GetAllUsers_InvalidFilePath_LogsException()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var service = new UserService(db);
                service.AddUser(new User { UserName = "u", Password = "p", UserPhotoPath = "invalid|path.png" });

                var sw = new StringWriter();
                var original = Console.Out;
                Console.SetOut(sw);
                try
                {
                    service.GetAllUsers();
                }
                finally
                {
                    Console.SetOut(original);
                }
                Assert.NotEqual(string.Empty, sw.ToString());
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }
    }
}
