using System;
using System.ComponentModel.DataAnnotations;

namespace api.Models.DTO;

public class LoginVM
{
  [Required(ErrorMessage = "Email is required")]
  [EmailAddress(ErrorMessage = "Email not valid")]
  public required string Email { get; set; }

  [Required(ErrorMessage = "Password is required")]
  public required string Password { get; set; }
}
