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
        Task<bool> ValidateUserAsync(string username, string password);
      
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
                .FirstOrDefaultAsync(u => u.Username == model.Username);

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
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Token oluşturma
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username)
            };

            var token = _tokenService.GenerateToken(claims);
            return token;
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public async Task<bool> ValidateUserAsync(string username, string password)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == username);
            if (user == null)
                return false;

            var isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            return isPasswordValid;
        }


        private string GenerateEmailToken(int userId)
        {
            // E-posta token'ı oluşturma işlemi
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("YourSecretKeyHere"); // Güvenlik anahtarı
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private int? DecodeEmailToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("YourSecretKeyHere"); // Güvenlik anahtarı
            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                }, out SecurityToken securityToken);

                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
                return userIdClaim != null ? int.Parse(userIdClaim.Value) : (int?)null;
            }
            catch
            {
                return null;
            }
        }

        private void SendConfirmationEmail(string email, string token)
        {
            // E-posta gönderme işlemi
            var confirmationLink = $"https://yourdomain.com/api/account/confirmemail?token={token}";

            var fromAddress = new MailAddress("no-reply@yourdomain.com", "YourAppName");
            var toAddress = new MailAddress(email);
            const string fromPassword = "yourpassword"; // Bu parola genellikle uygulamanın e-posta hesabına ait
            const string subject = "Confirm your email";
            string body = $"Please confirm your account by clicking this link: {confirmationLink}";

            var smtp = new SmtpClient
            {
                Host = "smtp.yourdomain.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }
        }
    }
}
