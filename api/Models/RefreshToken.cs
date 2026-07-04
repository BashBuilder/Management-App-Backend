using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models;

public class RefreshToken
{
  public int Id { get; set; }
  public required string UserId { get; set; }
  public required string Token { get; set; }
  public required string JwtId { get; set; }
  public bool IsRevoked { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdateAt { get; set; }
  public DateTime DateExpire { get; set; }

  [ForeignKey(nameof(UserId))]
  public required ApplicationUser User { get; set; }
}