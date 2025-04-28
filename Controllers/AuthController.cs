using BlogAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<Users> _userManager;
    private readonly IConfiguration _config;

    public AuthController(UserManager<Users> userManager, IConfiguration config)
    {
        _userManager = userManager;
        _config = config;
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult> GetLoggedInUser()
    {
        
        foreach (var claim in User.Claims)
        {
            Console.WriteLine($"Claim Type: {claim.Type}, Claim Value: {claim.Value}");
        }

        // Mendapatkan userId dari klaim (claims) yang ada di HttpContext
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User is not authenticated.");
        }

        // Mencari user berdasarkan userId
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return NotFound("User not found.");
        }

        // Mendapatkan daftar role dari pengguna
        var roles = await _userManager.GetRolesAsync(user);

        // Mengembalikan informasi pengguna beserta role-nya
        return Ok(new
        {
            user.UserName,
            user.Email,
            user.FullName,
            Roles = roles // Menambahkan roles ke response
        });
    }


    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var user = new Users
        {
            UserName = dto.Username,
            Email = dto.Email,
            FullName = dto.full_name
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "user");
            return Ok("Registrasi berhasil");
        }
        else
        {
            return BadRequest(result.Errors);
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _userManager.FindByNameAsync(dto.Username);
        if (user != null && await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            var token = GenerateJwtToken(user);
            return Ok(new { token });
        }

        return Unauthorized("Login gagal");
    }

    private string GenerateJwtToken(Users user)
    {
        var jwt = _config.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            // new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, string.Join(",", _userManager.GetRolesAsync(user).Result)),
            new Claim("lastRoleUpdate", user.LastRoleUpdate.ToString("O"))
        };

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(jwt["ExpiresInMinutes"])),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}