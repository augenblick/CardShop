
using CardShop.Enums;
using CardShop.Interfaces;
using CardShop.Logic;
using CardShop.Models.Request;
using CardShop.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CardShop.Controllers
{
    [Route("[controller]/[action]")]
    public class UserController : ControllerBase
    {

        private readonly IUserRepository _userRepository;
        private readonly TokenManager _tokenService;
        private readonly ILogger _logger;
        private readonly IShopManager _shopManager;

        public UserController(IUserRepository userRepository, TokenManager tokenService, ILogger<UserController> logger, IShopManager shopManager)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _logger = logger;
            _shopManager = shopManager;
        }


        [HttpPost]
        public async Task<IActionResult> Register(RegistrationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userInDb = await _userRepository.GetSecureUser(request.Username);
            
            if (userInDb != null)
            {
                return Problem("That Username is already in use!");
            }

            var newUser = await _userRepository.AddUser(request.Username, request.Password, request.Email, 25M, Role.User);

            if (newUser == null)
            {
                var errorMessage = $"For an unknown reason, the user could not be created.";
                _logger.LogError(errorMessage);
                return Problem(errorMessage);
            }

            await _shopManager.AllotStartingInventoryToUser(newUser.Username);

            request.Password = "[redacted]";
            return CreatedAtAction(nameof(Register), new { username = request.Username }, request);
        }


        [HttpPost]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            var userInDb = await _userRepository.GetSecureUser(request.Username);

            if (userInDb is null)
            {
                return Unauthorized("Username or password are incorrect.");
            }
            if (request.Password != userInDb.Password)
            {
                return Unauthorized("Username or password are incorrect.");
            }
            if (userInDb.Role == Role.Shop)
            {
                return Unauthorized("May not log in with shop account.");
            }

            var accessToken = _tokenService.CreateToken(userInDb);

            return Ok(new LoginResponse
            {
                Username = userInDb.Username,
                Token = accessToken,
            });
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<bool>> UpdatePassword(string existingPassword, string newPassword)
        {
            var userName = HttpContext?.User?.Identity?.Name;
            var user = await _userRepository.GetSecureUser(userName);

            if (string.IsNullOrWhiteSpace(user?.Username))
            {
                return Unauthorized();
            }

            if (existingPassword != user.Password)
            {
                return Unauthorized();
            }

            var updated = _userRepository.UpdateUserPassword(user.UserId, newPassword);

            return Ok(updated);
        }

    }
}
