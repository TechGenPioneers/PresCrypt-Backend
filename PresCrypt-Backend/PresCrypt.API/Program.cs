using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PresCrypt_Backend.PresCrypt.API.Controllers;
using PresCrypt_Backend.PresCrypt.Application.Services.AuthServices;
using PresCrypt_Backend.PresCrypt.Application.Services.DoctorServices;
using PresCrypt_Backend.PresCrypt.Application.Services.UserServices;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Correct the base path to the folder where appsettings.json is located
builder.Configuration
    .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "PresCrypt.API"))
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLogging(config =>
{
    config.AddConsole();
});


// Register services
builder.Services.AddScoped<IDoctorService, DoctorServices>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IJwtService, JwtService>(); // Scoped registration for JwtService



// Configure JWT Authentication
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true, // ? THIS MUST BE TRUE
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });


// Register controllers for Dependency Injection
builder.Services.AddScoped<PatientController>();
builder.Services.AddScoped<DoctorController>();
builder.Services.AddScoped<AdminController>();

// Set up Entity Framework DbContext with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure CORS to allow frontend access
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000") // Update this if frontend URL changes
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"Connection string: {connectionString}");

var app = builder.Build();

// Enable Swagger in Development environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware pipeline setup
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication(); // Authentication should come before Routing
app.UseAuthorization(); // Authorization after authentication
app.UseRouting(); // Routing middleware after auth
app.MapControllers(); // Map Controllers to Routes
app.Run();
