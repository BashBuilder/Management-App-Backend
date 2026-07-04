using System;
using api.Models.DTO;
using Microsoft.AspNetCore.Identity;

namespace api.Data;

public static class AppDbInitializer
{
  public static async Task SeedRoles(this WebApplication app)
  {
    var scope = app.Services.CreateScope();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    if (!await roleManager.RoleExistsAsync(UserRolesDto.Admin))
    {
      await roleManager.CreateAsync(new IdentityRole(UserRolesDto.Admin));
    }
    if (!await roleManager.RoleExistsAsync(UserRolesDto.People))
    {
      await roleManager.CreateAsync(new IdentityRole(UserRolesDto.People));
    }
    if (!await roleManager.RoleExistsAsync(UserRolesDto.Member))
    {
      await roleManager.CreateAsync(new IdentityRole(UserRolesDto.Member));
    }

  }
}
