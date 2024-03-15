using CardShop.Repositories.Models;

namespace CardShop.Interfaces
{
    public interface IUserManager
    {
        Task<User> GetUser(int userId);
        Task<bool> SetUserBalance(int userId, decimal newBalance);
    }
}
