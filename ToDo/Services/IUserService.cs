using ToDo.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Net.Mail;
using System.Net;
using System.IdentityModel.Tokens.Jwt;

namespace ToDo.Services
{
    public interface IUserService
    {
        Task<string> RegisterAsync(RegisterModel model);
        Task<bool> ValidateUserAsync(string username, string password, string email);
    }


    public class UserService : IUserService
    {
        private readonly TodoContext _context;
        private readonly TokenService _tokenService;

        public UserService(TodoContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        public async Task<string> RegisterAsync(RegisterModel model)
        {
            // Kullanıcı adı veya e-posta kontrolü
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == model.Username || u.Email == model.Email);

            if (existingUser != null)
            {
                return null; // Kullanıcı adı veya e-posta zaten mevcut
            }

            // Yeni kullanıcı oluştur
            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = HashPassword(model.Password),
                IsEmailConfirmed = false // E-posta henüz doğrulanmadı
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return "Registration successful! Please check your email to confirm your account.";
        }


        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }


        public async Task<bool> ValidateUserAsync(string username, string password, string email)
        {
            var user = await _context.Users
                                     .SingleOrDefaultAsync(u => u.Username == username && u.Email == email);


            var isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            return isPasswordValid;
        }

    }
}
