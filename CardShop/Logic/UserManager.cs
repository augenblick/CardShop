using CardShop.Enums;
using CardShop.Interfaces;
using CardShop.Repositories.Models;
using Microsoft.AspNetCore.Identity;

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

        public async Task<User> GetUser(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) { return new User(); }
            var user = await _userRepository.GetUser(username);

            return user ?? new User();
        }

        public async Task<User?> GetUser(HttpContext context)
        {
            var userName = context?.User?.Identity?.Name;
            var user = await GetUser(userName);

            if (string.IsNullOrWhiteSpace(user.Username))
            {
                return null;
            }

            return user;
        }

        public async Task<User> AddUser(string username, string password, string email = null, decimal balance = 0, Role role = Role.User, int? userId = null)
        {
            if (string.IsNullOrWhiteSpace(username)) { return new User(); }
            var existing = await _userRepository.GetUser(username);

            if (!string.IsNullOrWhiteSpace(existing?.Username))
            {
                _logger.LogError($"Username '{username}' already exists!");
                return new User();
            }

            return await _userRepository.AddUser(username, password, email, balance, role, userId);
        }


        public async Task<bool> DeleteUser(int userId)
        {
            var user = await _userRepository.GetSecureUser(userId);

            if (user.Role == Role.Shop)
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

        public async Task<User> SetUserRole(int userId, Role role)
        {
            var userWasUpdated = await _userRepository.SetUserRole(userId, role);

            if (!userWasUpdated)
            {
                return new User();
            }

            return await _userRepository.GetUser(userId);
        }

        public async Task<User> SetUserRole(string userName, Role role)
        {
            var user = await GetUser(userName);

            if (string.IsNullOrWhiteSpace(user.Username))
            {
                return new User();
            }

            var userWasUpdated = await _userRepository.SetUserRole(user.UserId, role);

            if (!userWasUpdated)
            {
                return new User();
            }

            return await _userRepository.GetUser(user.UserId);
        }
    }
}
