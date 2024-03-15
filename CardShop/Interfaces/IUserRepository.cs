using CardShop.Repositories.Models;

namespace CardShop.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetUser(int userId);
        Task<bool> SetUserBalance(int userId, decimal newBalance);
    }
}
