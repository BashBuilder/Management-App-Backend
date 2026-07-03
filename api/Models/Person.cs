using System;
using System.ComponentModel.DataAnnotations;

namespace api.Models;

public class Person
{
  public int Id { get; set; }
  [Required]
  [MaxLength(30)]
  public required string FirstName { get; set; }
  [Required]
  [MaxLength(30)]
  public required string LastName { get; set; }
}
