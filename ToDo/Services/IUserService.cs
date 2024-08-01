using ToDo.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ToDo.Services
{
    public interface IUserService
    {
        Task<string> RegisterAsync(RegisterModel model); // Token döndürmek için türü string olarak değiştirdim
        Task<bool> ValidateUserAsync(string username, string password);
    }

    public class UserService : IUserService
    {
        private readonly TodoContext _context;
        private readonly TokenService _tokenService; // TokenService ekleniyor

        public UserService(TodoContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService; // TokenService üzerinden bağımlılık alınıyor
        }

        public async Task<string> RegisterAsync(RegisterModel model)
        {
            // Kullanıcıyı kontrol et
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == model.Username );

            if (existingUser != null)
            {
                return null; // Kullanıcı adı veya e-posta zaten mevcut
            }

            // Yeni kullanıcı oluştur
            var user = new User
            {
                Username = model.Username,
                PasswordHash = HashPassword(model.Password) // Parolayı hash'leyin
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Token oluşturma
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username)
                // Diğer gerekli claim'leri ekleyin
            };

            var token = _tokenService.GenerateToken(claims);
            return token; // Kayıt başarılı ve token döndürülüyor
        }

        private string HashPassword(string password)
        {
            // Parola hashleme işlemi için BCrypt kullanın
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public async Task<bool> ValidateUserAsync(string username, string password)
        {
            // Kullanıcıyı veritabanında bul
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == username);
            if (user == null)
                return false;

            // Şifreyi doğrula
            var isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            return isPasswordValid;
        }
    }
}
