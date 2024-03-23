
using CardShop.Enums;
using CardShop.Interfaces;
using CardShop.Logic;
using CardShop.Models.Request;
using CardShop.Models.Response;
using Microsoft.AspNetCore.Mvc;

namespace CardShop.Controllers
{
    public class UserController : ControllerBase
    {

        //private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly TokenManager _tokenService;
        private readonly ILogger _logger;

        public UserController(IUserRepository userRepository, TokenManager tokenService, ILogger<UserController> logger)
        {
            _userRepository = userRepository;
            //_userManager = userManager;
            _tokenService = tokenService;
            _logger = logger;
        }


        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(RegistrationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //var result = await _userManager.CreateAsync(
            //    new ApplicationUser { UserName = request.Username, Email = request.Email },
            //    request.Password!
            //);

            var userInDb = _userRepository.GetSecureUser(request.Username);
            
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

            request.Password = "";
            return CreatedAtAction(nameof(Register), new { username = request.Username }, request);
        }


        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //var managedUser = await _userManager.FindByEmailAsync(request.Email!);
            //if (managedUser == null)
            //{
            //    return BadRequest("Bad credentials");
            //}

            //var isPasswordValid = await _userManager.CheckPasswordAsync(managedUser, request.Password!);
            //if (!isPasswordValid)
            //{
            //    return BadRequest("Bad credentials");
            //}

            var userInDb = await _userRepository.GetSecureUser(request.Username);

            if (userInDb is null)
            {
                return Unauthorized();
            }
            if (request.Password != userInDb.Password)
            {
                return Unauthorized();
            }

            var accessToken = _tokenService.CreateToken(userInDb);
            //await _context.SaveChangesAsync();

            return Ok(new LoginResponse
            {
                Username = userInDb.Username,
                Token = accessToken,
            });
        }
    }
}
