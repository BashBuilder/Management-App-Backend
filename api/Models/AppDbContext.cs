using System;
using Microsoft.EntityFrameworkCore;

namespace api.Models;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
  public DbSet<Person> People { get; set; }
}
