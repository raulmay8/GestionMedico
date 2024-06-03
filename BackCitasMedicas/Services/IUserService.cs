using BackCitasMedicas.Data;
using BackCitasMedicas.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BackCitasMedicas.Services
{
    public interface IUserService
    {
        Task<User> Register(User user, string Password);
        Task<User> Login(string email, string password);
        string GenerateJwtToken(User user);
    }

    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public UserService(ApplicationDbContext context, IConfiguration configuration, IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<User> Register(User user, string password)
        {
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                throw new Exception("Correo electrónico en uso");

            user.Password = BCrypt.Net.BCrypt.HashPassword(password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Enviar correo electrónico de confirmación
            var token = GenerateJwtToken(user);
            var confirmationLink = $"https://localhost:5001/api/auth/confirm-email?token={token}";
            var emailBody = $@"
                <p>Por favor confirma tu cuenta ingresando al siguiente enlace <a href='{confirmationLink}'>aquí</a>.</p>
                <p>Si recibió por error este correo, favor de ignorar.</p>
            ";

            _emailService.SendEmail(user.Email, "Confirma tu cuenta", emailBody);

            return user;
        }

        public async Task<User> Login(string email, string password)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
                throw new Exception("Credenciales no válidas.");

            return user;
        }

        public string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(double.Parse(_configuration["Jwt:DurationInMinutes"])),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
