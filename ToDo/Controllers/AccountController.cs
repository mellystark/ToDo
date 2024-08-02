using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ToDo.Models;
using ToDo.Services;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly TokenService _tokenService;
    private readonly IUserService _userService;

    public AccountController(TokenService tokenService, IUserService userService)
    {
        _tokenService = tokenService;
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var token = await _userService.RegisterAsync(model);
        if (!string.IsNullOrEmpty(token))
        {
            return Ok(new { Token = token }); // Kayıt başarılı, token döndürülüyor
        }

        return BadRequest("Kullanıcı adı veya e-posta zaten mevcut.");
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var isValidUser = await _userService.ValidateUserAsync(loginModel.Username, loginModel.Password, loginModel.Email);
        if (!isValidUser)
        {
            return Unauthorized("Geçersiz kullanıcı adı, şifre veya e-posta.");
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, loginModel.Username),
            new Claim(ClaimTypes.Email, loginModel.Email)
        };

        var token = _tokenService.GenerateToken(claims);
        return Ok(new { Token = token });
    }
}
