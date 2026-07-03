namespace api.Extensions;

public static class CorsServiceExtensions
{
  public const string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

  public static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
  {
    services.AddCors(options =>
      {
        options.AddPolicy(name: MyAllowSpecificOrigins,
          policy =>
          {
            policy.WithOrigins("http://localhost:3000")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
          });
      });
    return services;
  }
}
