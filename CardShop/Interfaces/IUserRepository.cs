using CardShop.Repositories.Models;

namespace CardShop.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetUser(int userId);
        Task<User> GetUser(string username);
        Task<User> AddUser(string username, decimal balance);
        Task<bool> DeleteUser(int userId);
        Task<bool> SetUserBalance(int userId, decimal newBalance);
    }
}
