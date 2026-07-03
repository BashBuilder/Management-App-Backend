using api.Models;
using api.Models.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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



    }
}
