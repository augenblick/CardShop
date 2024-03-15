using CardShop.Interfaces;
using CardShop.Repositories.Models;

namespace CardShop.Logic
{
    public class UserManager : IUserManager
    {
        private readonly IUserRepository _userRepository;

        public UserManager(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User> GetUser(int userId)
        {
            return await _userRepository.GetUser(userId);
        }

        public async Task<bool> SetUserBalance(int userId, decimal newBalance)
        {
            return await _userRepository.SetUserBalance(userId, newBalance);
        }
    }
}
