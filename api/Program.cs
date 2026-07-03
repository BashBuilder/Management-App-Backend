using api.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();


string connectionString = builder.Configuration.GetConnectionString("Default")
                          ?? throw new ArgumentNullException("Connection string not found.");

builder.Services.AddSqlite<AppDbContext>(connectionString);

var app = builder.Build();


// Add Middleware to the HTTP request pipeline.
app.MapGet("/health", () => "Server is healthy");
app.MapControllers();

app.Run();
