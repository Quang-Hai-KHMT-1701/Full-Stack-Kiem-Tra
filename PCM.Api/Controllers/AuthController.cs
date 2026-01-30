using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PCM.Api.Data;
using PCM.Api.DTOs.Auth;
using PCM.Api.Models.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _config;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context,
        IConfiguration config)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _config = config;
    }

    // ==========================
    // POST: api/auth/register
    // ==========================
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        // Kiểm tra email đã tồn tại
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            return BadRequest(new { message = "Email đã được đăng ký" });

        // Tạo user
        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
            return BadRequest(new { message = "Đăng ký thất bại", errors = result.Errors });

        // Gán role Member mặc định
        if (await _roleManager.RoleExistsAsync("Member"))
        {
            await _userManager.AddToRoleAsync(user, "Member");
        }

        // Tạo Member profile liên kết với User
        var member = new Member
        {
            FullName = dto.FullName ?? dto.Email.Split('@')[0],
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber ?? "",
            UserId = user.Id,
            JoinDate = DateTime.Now,
            IsActive = true,
            RankLevel = 1.0,
            TotalMatches = 0,
            WinMatches = 0,
            CreatedDate = DateTime.Now
        };

        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        // Tạo token và trả về
        var roles = await _userManager.GetRolesAsync(user);
        var token = GenerateJwtToken(user, roles);

        return Ok(new
        {
            message = "Đăng ký thành công",
            token,
            userId = user.Id,
            memberId = member.Id,
            email = user.Email,
            fullName = member.FullName,
            roles = roles
        });
    }


    // ==========================
    // POST: api/auth/login
    // ==========================
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Unauthorized(new { message = "Email không tồn tại" });

        var validPassword = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!validPassword)
            return Unauthorized(new { message = "Sai mật khẩu" });

        // Lấy Member profile
        var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == user.Id);

        // Lấy roles của user
        var roles = await _userManager.GetRolesAsync(user);
        var token = GenerateJwtToken(user, roles);

        return Ok(new
        {
            token,
            userId = user.Id,
            memberId = member?.Id,
            email = user.Email,
            fullName = member?.FullName ?? user.UserName,
            phoneNumber = member?.PhoneNumber,
            roles = roles,
            rankLevel = member?.RankLevel ?? 1.0,
            totalMatches = member?.TotalMatches ?? 0,
            winMatches = member?.WinMatches ?? 0
        });
    }

    // ==========================
    // GET: api/auth/me - Lấy thông tin user hiện tại
    // ==========================
    [HttpGet("me")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { message = "User không tồn tại" });

        var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == user.Id);
        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new
        {
            userId = user.Id,
            memberId = member?.Id,
            email = user.Email,
            fullName = member?.FullName ?? user.UserName,
            phoneNumber = member?.PhoneNumber,
            roles = roles,
            rankLevel = member?.RankLevel ?? 1.0,
            totalMatches = member?.TotalMatches ?? 0,
            winMatches = member?.WinMatches ?? 0,
            joinDate = member?.JoinDate,
            isActive = member?.IsActive ?? true
        });
    }

    // ==========================
    // JWT Generator
    // ==========================
    private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
    {
        var jwt = _config.GetSection("Jwt");

        var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

        // Thêm roles vào claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwt["Key"]!)
        );

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                double.Parse(jwt["DurationInMinutes"]!)
            ),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}