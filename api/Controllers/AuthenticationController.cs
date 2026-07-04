using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using api.Models;
using api.Models.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        AppDbContext dbContext,
        IConfiguration configuration

    ) : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly AppDbContext _dbContext = dbContext;
        private readonly IConfiguration _configuration = configuration;

        [HttpPost("register-user")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterVM payload)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Please, provide all required fields");
                }
                var userExists = await _userManager.FindByEmailAsync(payload.Email);

                if (userExists is not null)
                {
                    return BadRequest($"User {payload.Email} already exists");
                }
                ApplicationUser newUser = new()
                {
                    Email = payload.Email,
                    UserName = payload.Username,
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                var result = await _userManager.CreateAsync(newUser, payload.Password);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description);
                    return BadRequest(new { Errors = errors });
                }

                return Created(nameof(RegisterUser), $"User {payload.Email} Created");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message ?? "Error while trying to create new user");
            }
        }

        [HttpPost("login-user")]
        public async Task<IActionResult> LoginUser([FromBody] LoginVM payload)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Please, provide all required fields");
            }
            var user = await _userManager.FindByEmailAsync(payload.Email);
            if (user is not null && await _userManager.CheckPasswordAsync(user, payload.Password))
            {
                var tokenValue = await GenerateJwtToken(user);
                return Ok(tokenValue);
            }

            return Unauthorized("Invalid Credentials");
        }

        private async Task<AuthResultVM> GenerateJwtToken(ApplicationUser user)
        {
            var authClaims = new List<Claim>()
            {
                new(ClaimTypes.Name, user.UserName),
                new(ClaimTypes.NameIdentifier, user.Id ),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(JwtRegisteredClaimNames.Sub, user.Email ),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email ),
            };


            var authSigningKey = new SymmetricSecurityKey(
                  Encoding.ASCII.GetBytes(_configuration["JWT:Secret"]
                      ?? throw new ArgumentNullException("Jwt secret not found")));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                expires: DateTime.UtcNow.AddMinutes(5),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
            var refreshToken = new RefreshToken()
            {
                JwtId = token.Id,
                IsRevoked = false,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow,
                Token = Guid.NewGuid().ToString() + "-" + Guid.NewGuid().ToString(),
                DateExpire = DateTime.UtcNow.AddMonths(6),
                User = user
            };

            await _dbContext.RefreshTokens.AddAsync(refreshToken);
            await _dbContext.SaveChangesAsync();

            var response = new AuthResultVM()
            {
                Token = jwtToken,
                RefreshToken = refreshToken.Token,
                ExpiresAt = token.ValidTo
            };

            return response;
        }
    }
}
