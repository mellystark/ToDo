using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using ToDo.Models;
using ToDo.Services;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly TokenService _tokenService;
    private readonly IUserService _userService; // IUserService'i ekleyin

    public AccountController(TokenService tokenService, IUserService userService)
    {
        _tokenService = tokenService;
        _userService = userService; // Kullanıcı servisini başlatın
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        if (ModelState.IsValid)
        {
            var token = await _userService.RegisterAsync(model);
            if (!string.IsNullOrEmpty(token))
            {
                return Ok(new { Token = token }); // Kayıt başarılı, token döndürülüyor
            }
            return BadRequest("Kullanıcı adı veya e-posta zaten mevcut.");
        }
        return BadRequest(ModelState);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var isValidUser = await _userService.ValidateUserAsync(loginModel.Username, loginModel.Password);
        if (!isValidUser)
        {
            return Unauthorized("Geçersiz kullanıcı adı veya şifre.");
        }

        var claims = new[]
        {
        new Claim(ClaimTypes.Name, loginModel.Username),
        // Diğer gerekli claim'leri ekleyin
    };

        var token = _tokenService.GenerateToken(claims);
        return Ok(new { Token = token });
    }
}
