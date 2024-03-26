using CardShop.Enums;
using CardShop.Repositories.Models;
using System.Threading.Tasks;

namespace CardShop.Interfaces
{
    public interface IUserManager
    {
        Task<User> GetUser(int userId);
        Task<User> GetUser(string username);
        Task<User> SetUserRole(int userId, Role role);
        Task<User> SetUserRole(string username, Role role);
        Task<User> AddUser(string username, string password, string email = null, decimal balance = 0, Role role = Role.User);
        Task<bool> DeleteUser(int userId);
        Task<bool> SetUserBalance(int userId, decimal newBalance);
    }
}
