using api.Models;
using Microsoft.AspNetCore.Identity;

namespace api.Extensions;

public static class IdentityServiceExtensions
{
  public static IServiceCollection AddIdentityConfiguration(this IServiceCollection services)
  {
    services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

    return services;
  }
}
