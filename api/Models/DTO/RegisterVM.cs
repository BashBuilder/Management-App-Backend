using System;
using System.ComponentModel.DataAnnotations;

namespace api.Models.DTO;

public class RegisterVM
{
  [Required(ErrorMessage = "Username is required")]
  public required string Username { get; set; }

  [Required(ErrorMessage = "Email is required")]
  [EmailAddress(ErrorMessage = "Email not valid")]
  public required string Email { get; set; }

  [Required(ErrorMessage = "Password is required")]
  public required string Password { get; set; }
  [Required(ErrorMessage = "Role is required")]
  public required string Role { get; set; }
}
