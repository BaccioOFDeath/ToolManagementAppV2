using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Utilities.Helpers;
using ToolManagementAppV2.Interfaces;
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
                using var factory = LoggerFactory.Create(builder => builder.AddProvider(new ListLoggerProvider(logs)));
                var db = new DatabaseService(dbPath, factory.CreateLogger<DatabaseService>());
                IUserService service = new UserService(db, new ApplicationUserContext(), factory.CreateLogger<UserService>());
                var original = PathHelper.Logger;
                PathHelper.Configure(factory.CreateLogger<PathHelper>());
                try
                {
                    service.AddUser(new User { UserName = "u", PasswordHash = "Strong1!", UserPhotoPath = "pack://application:,,,/Resources/NoImage.png" });
                    service.GetAllUsers();
                }
                finally
                {
                    PathHelper.Configure(original);
                }
                Assert.Empty(logs);
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
                using var factory = LoggerFactory.Create(builder => builder.AddProvider(new ListLoggerProvider(logs)));
                var db = new DatabaseService(dbPath, factory.CreateLogger<DatabaseService>());
                IUserService service = new UserService(db, new ApplicationUserContext(), factory.CreateLogger<UserService>());
                var original = PathHelper.Logger;
                PathHelper.Configure(factory.CreateLogger<PathHelper>());
                try
                {
                    service.AddUser(new User { UserName = "u", PasswordHash = "Strong1!", UserPhotoPath = "invalid|path.png" });
                    service.GetAllUsers();
                }
                finally
                {
                    PathHelper.Configure(original);
                }
                Assert.Empty(logs);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }
    }
}
