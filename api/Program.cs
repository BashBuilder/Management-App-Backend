using api.Models;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
  options.AddPolicy(name: MyAllowSpecificOrigins,
                    policy =>
                    {
                      policy.WithOrigins("http://localhost:3000")
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
});


string connectionString = builder.Configuration.GetConnectionString("Default")
                          ?? throw new ArgumentNullException("Connection string not found.");

builder.Services.AddSqlite<AppDbContext>(connectionString);

var app = builder.Build();


app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors(MyAllowSpecificOrigins);


// Add Middleware to the HTTP request pipeline.
app.MapGet("/health", () => "Server is healthy");
app.MapControllers();

app.Run();
