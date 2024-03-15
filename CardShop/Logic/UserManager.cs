using CardShop.Interfaces;
using CardShop.Repositories.Models;

namespace CardShop.Logic
{
    public class UserManager : IUserManager
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger _logger;

        public UserManager(IUserRepository userRepository, ILogger<UserManager> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<User> GetUser(int userId)
        {
            return await _userRepository.GetUser(userId);
        }

        public async Task<User> AddUser(string userName, decimal balance)
        {
            return await _userRepository.AddUser(userName, balance);
        }

        public async Task<bool> DeleteUser(int userId)
        {
            if (userId == 0)
            {
                _logger.LogError("Deletion the Shop Keeper from the userlist ist verboten!");
                return false;
            }
            return await _userRepository.DeleteUser(userId);
        }

        public async Task<bool> SetUserBalance(int userId, decimal newBalance)
        {
            return await _userRepository.SetUserBalance(userId, newBalance);
        }
    }
}
