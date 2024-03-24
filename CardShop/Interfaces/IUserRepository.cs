using CardShop.Enums;
using CardShop.Repositories.Models;

namespace CardShop.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetUser(int userId);
        Task<User> GetUser(string username);
        Task<SecureUser> GetSecureUser(int userId);
        Task<SecureUser> GetSecureUser(string username);
        Task<User> AddUser(string username, string password, string email = null, decimal balance = 0m, Role role = Role.User);
        Task<bool> DeleteUser(int userId);
        Task<bool> SetUserBalance(int userId, decimal newBalance);
        Task<bool> SetUserRole(int userId, Role role);
    }
}
