using System.IdentityModel.Tokens.Jwt;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using api.Models;
using api.Models.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        AppDbContext dbContext,
        IConfiguration configuration,
        TokenValidationParameters tokenValidationParameters

    ) : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly AppDbContext _dbContext = dbContext;
        private readonly IConfiguration _configuration = configuration;

        // Refresh Tokens
        private readonly TokenValidationParameters _tokenValidationParameters = tokenValidationParameters;

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

                switch (payload.Role)
                {
                    case "Admin":
                        await _userManager.AddToRoleAsync(newUser, UserRolesDto.Admin);
                        break;
                    case "People":
                        await _userManager.AddToRoleAsync(newUser, UserRolesDto.People);
                        break;
                    case "Member":
                        await _userManager.AddToRoleAsync(newUser, UserRolesDto.Member);
                        break;
                    default:
                        await _userManager.AddToRoleAsync(newUser, UserRolesDto.Member);
                        break;
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
                var tokenValue = await GenerateJwtTokenAsync(user);
                return Ok(tokenValue);
            }

            return Unauthorized("Invalid Credentials");
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequestVm payload)
        {
            try
            {
                var result = await VerifyAndGenerateTokenAsync(payload);
                if (result == null) return BadRequest("Invalid Token");

                return Ok(result);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message ?? "Error validating Token");
            }
        }

        private async Task<AuthResultVM> VerifyAndGenerateTokenAsync(TokenRequestVm payload)
        {
            try
            {
                var jwtTokenHandler = new JwtSecurityTokenHandler();
                // check 1 - check jwt token format
                var tokenInVerification = jwtTokenHandler.ValidateToken(payload.Token, _tokenValidationParameters, out var validatedToken);

                // check 2 - Encryption algorithm
                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);

                    if (result == false) return null;
                }
                // check 3 - Validate expiry date
                var utcExpiryDate = long.Parse(tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
                var expiryDate = UnixTimeStampToDateTimeInUTC(utcExpiryDate);
                if (expiryDate > DateTime.UtcNow) throw new Exception("Token has not expired yet");

                // check 4 - Refresh token exists in the Db
                var dbRefreshToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(refreshToken => refreshToken.Token == payload.RefreshToken);
                if (dbRefreshToken is null) throw new Exception("Refresh token does not exists");
                else
                {
                    // check 5 - Validate Id
                    var jti = tokenInVerification.Claims.FirstOrDefault(token => token.Type == JwtRegisteredClaimNames.Jti).Value;
                    if (dbRefreshToken.JwtId != jti) throw new Exception("Token does not match");

                    // check 6 - Refresh token expiration
                    if (dbRefreshToken.DateExpire <= DateTime.UtcNow) throw new Exception("Your refresh token has expired, please login");

                    // check 7 - Refresh token revoked
                    if (dbRefreshToken.IsRevoked) throw new Exception("Refresh token is revoked");

                    // Generate new token (with existing refresh token)
                    var dbUserData = await _userManager.FindByIdAsync(dbRefreshToken.UserId);
                    var newTokenResponse = await GenerateJwtTokenAsync(dbUserData, payload.RefreshToken);

                    return newTokenResponse;
                }
            }
            catch (SecurityTokenException)
            {
                var dbRefreshToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(refreshToken => refreshToken.Token == payload.RefreshToken);
                // Generate new token (with existing refresh token)
                var dbUserData = await _userManager.FindByIdAsync(dbRefreshToken.UserId);
                var newTokenResponse = await GenerateJwtTokenAsync(dbUserData, payload.RefreshToken);

                return newTokenResponse;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private DateTime UnixTimeStampToDateTimeInUTC(long unixTimeStamp)
        {
            var dateTimeVal = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTimeVal = dateTimeVal.AddSeconds(unixTimeStamp);

            return dateTimeVal;

        }


        private async Task<AuthResultVM> GenerateJwtTokenAsync(ApplicationUser user, [Optional] string existingRefreshToken)
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

            // Add User Roles
            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new(ClaimTypes.Role, userRole));
            }


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

            if (string.IsNullOrEmpty(existingRefreshToken))
            {
                await _dbContext.RefreshTokens.AddAsync(refreshToken);
                await _dbContext.SaveChangesAsync();
            }


            var response = new AuthResultVM()
            {
                Token = jwtToken,
                RefreshToken = string.IsNullOrEmpty(existingRefreshToken) ? refreshToken.Token : existingRefreshToken,
                ExpiresAt = token.ValidTo
            };

            return response;
        }
    }
}
