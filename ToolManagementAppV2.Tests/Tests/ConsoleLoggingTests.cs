using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Windows.Controls;
using ToolManagementAppV2;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Utilities.Converters;
using ToolManagementAppV2.Utilities.Helpers;
using ToolManagementAppV2.Models.Domain;
using Xunit;

namespace ToolManagementAppV2.Tests
{
    public class ConsoleLoggingTests
    {
        [Fact]
        public void PathHelper_InvalidPath_LogsException()
        {
            var sw = new StringWriter();
            var original = Console.Out;
            Console.SetOut(sw);
            try
            {
                var result = PathHelper.GetAbsolutePath("invalid|path");
                Assert.Null(result);
            }
            finally
            {
                Console.SetOut(original);
            }
            Assert.NotEqual(string.Empty, sw.ToString());
        }

        [Fact]
        public void NullToDefaultImageConverter_InvalidResource_LogsException()
        {
            var converter = new NullToDefaultImageConverter();
            var method = typeof(NullToDefaultImageConverter).GetMethod("LoadFromResource", BindingFlags.NonPublic | BindingFlags.Instance);
            var sw = new StringWriter();
            var original = Console.Out;
            Console.SetOut(sw);
            try
            {
                method.Invoke(converter, new object[] { "NoSuchFile.png" });
            }
            finally
            {
                Console.SetOut(original);
            }
            Assert.NotEqual(string.Empty, sw.ToString());
        }

        [Fact]
        public void ShowError_LogsException()
        {
            var window = (MainWindow)FormatterServices.GetUninitializedObject(typeof(MainWindow));
            var method = typeof(MainWindow).GetMethod("ShowError", BindingFlags.NonPublic | BindingFlags.Instance);
            var sw = new StringWriter();
            var original = Console.Out;
            Console.SetOut(sw);
            try
            {
                method.Invoke(window, new object[] { "Error", new Exception("fail") });
            }
            finally
            {
                Console.SetOut(original);
            }
            Assert.Contains("fail", sw.ToString());
        }

        [Fact]
        public void RefreshUserList_InvalidPhoto_LogsException()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var userService = new UserService(db);
                userService.AddUser(new User { UserName = "u", Password = "p", UserPhotoPath = "pack://application:,,,/Resources/NoFile.png" });
                var window = (MainWindow)FormatterServices.GetUninitializedObject(typeof(MainWindow));
                var list = new ListView();
                typeof(MainWindow).GetField("_userService", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(window, userService);
                typeof(MainWindow).GetField("UserList", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(window, list);

                var sw = new StringWriter();
                var original = Console.Out;
                Console.SetOut(sw);
                try
                {
                    typeof(MainWindow).GetMethod("RefreshUserList", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(window, null);
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
        public void BackupDatabase_LogsException()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var sw = new StringWriter();
                var original = Console.Out;
                Console.SetOut(sw);
                Assert.Throws<IOException>(() => db.BackupDatabase("invalid|path.db"));
                Console.SetOut(original);
                Assert.NotEqual(string.Empty, sw.ToString());
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }
    }
}
