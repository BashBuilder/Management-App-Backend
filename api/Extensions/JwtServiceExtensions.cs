using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace api.Extensions;

public static class JwtServiceExtensions
{
  public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
  {
    var tokenValidationParameter = new TokenValidationParameters
    {
      ValidateIssuerSigningKey = true,
      IssuerSigningKey = new SymmetricSecurityKey(
                  Encoding.ASCII.GetBytes(configuration["JWT:Secret"]
                      ?? throw new ArgumentNullException("Jwt secret not found"))),
      ValidateIssuer = true,
      ValidIssuer = configuration["JWT:Issuer"]
                  ?? throw new ArgumentNullException("Issuer not found"),
      ValidateAudience = true,
      ValidAudience = configuration["JWT:Audience"]
                  ?? throw new ArgumentNullException("Audience not found"),
      ValidateLifetime = true,
      ClockSkew = TimeSpan.Zero
    };

    services.AddSingleton(tokenValidationParameter);

    services.AddAuthentication(options =>
    {
      options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
      options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
      options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
      options.SaveToken = true;
      options.RequireHttpsMetadata = false;
      options.TokenValidationParameters = tokenValidationParameter;
    });

    return services;
  }
}
