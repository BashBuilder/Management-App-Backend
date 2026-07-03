using System;
using Microsoft.AspNetCore.Identity;

namespace api.Models;

public class ApplicationUser : IdentityUser
{
  public string Custom { get; set; } = string.Empty;
}
