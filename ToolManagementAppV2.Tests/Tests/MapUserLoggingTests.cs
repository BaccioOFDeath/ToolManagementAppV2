using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using ToolManagementAppV2;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Users;
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
                var db = new DatabaseService(dbPath);
                IUserService service = new UserService(db, new ApplicationUserContext());
                service.AddUser(new User { UserName = "u", Password = "p", UserPhotoPath = "pack://application:,,,/Resources/NoImage.png" });

                var logs = new List<LogEntry>();
                var factory = LoggerFactory.Create(builder => builder.AddProvider(new ListLoggerProvider(logs)));
                var originalFactory = App.LoggerFactory;
                App.LoggerFactory = factory;
                try
                {
                    service.GetAllUsers();
                }
                finally
                {
                    App.LoggerFactory = originalFactory;
                    factory.Dispose();
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
                var db = new DatabaseService(dbPath);
                IUserService service = new UserService(db, new ApplicationUserContext());
                service.AddUser(new User { UserName = "u", Password = "p", UserPhotoPath = "invalid|path.png" });

                var logs = new List<LogEntry>();
                var factory = LoggerFactory.Create(builder => builder.AddProvider(new ListLoggerProvider(logs)));
                var originalFactory = App.LoggerFactory;
                App.LoggerFactory = factory;
                try
                {
                    service.GetAllUsers();
                }
                finally
                {
                    App.LoggerFactory = originalFactory;
                    factory.Dispose();
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
