using api.Models;
namespace api.Extensions;

public static class DatabaseServiceExtensions
{
  public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
  {
    string connectionString = configuration.GetConnectionString("Default")
      ?? throw new ArgumentNullException("Connection string not found.");

    services.AddSqlite<AppDbContext>(connectionString);
    return services;
  }

}
