using CardShop.Repositories.Models;

namespace CardShop.Interfaces
{
    public interface IUserManager
    {
        Task<User> GetUser(int userId);
        Task<User> GetUser(string username);
        Task<User> AddUser(string userName, decimal balance = 0.0M);
        Task<bool> DeleteUser(int userId);
        Task<bool> SetUserBalance(int userId, decimal newBalance);
    }
}
