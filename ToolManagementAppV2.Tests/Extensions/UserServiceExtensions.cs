using System.Collections.Generic;
using System.Threading.Tasks;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Models.Domain;

namespace ToolManagementAppV2.Tests.Extensions
{
    public static class UserServiceExtensions
    {
        public static List<User> GetAllUsers(this IUserService service) =>
            service.GetAllUsersAsync().GetAwaiter().GetResult();

        public static User? AuthenticateUser(this IUserService service, string userName, string password) =>
            service.AuthenticateUserAsync(userName, password).GetAwaiter().GetResult();

        public static void UpdateUser(this IUserService service, User user) =>
            service.UpdateUserAsync(user).GetAwaiter().GetResult();
    }
}
