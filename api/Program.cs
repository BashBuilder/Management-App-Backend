using api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddCorsConfiguration();
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddIdentityConfiguration();
builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();

// Add Middlware for the proper application running
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors(CorsServiceExtensions.MyAllowSpecificOrigins);
app.MapGet("/health", () => "Server is healthy");
app.MapControllers();

app.Run();
