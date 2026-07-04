using System;
using System.ComponentModel.DataAnnotations;

namespace api.Models.DTO;

public class TokenRequestVm
{
  [Required(ErrorMessage = "Token is required")]
  public required string Token { get; set; }
  [Required(ErrorMessage = "Refresh Token is required")]
  public required string RefreshToken { get; set; }
}
