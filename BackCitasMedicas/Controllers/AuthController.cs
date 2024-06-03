using BackCitasMedicas.Models;
using BackCitasMedicas.Services;
using Microsoft.AspNetCore.Mvc;

namespace BackCitasMedicas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var user = new User
            {
                Username = request.Username,
                Email = request.Email
            };

            try
            {
                var createdUser = await _userService.Register(user, request.Password);
                return Ok(createdUser);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var user = await _userService.Login(request.Email, request.Password);
                var token = _userService.GenerateJwtToken(user);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }
    }
}
