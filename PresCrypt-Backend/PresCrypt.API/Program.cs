using Microsoft.EntityFrameworkCore;
using PresCrypt_Backend.PresCrypt.Application.Services.AdminServices;
using PresCrypt_Backend.PresCrypt.Application.Services.AdminServices.Impl;
using PresCrypt_Backend.PresCrypt.Application.Services.AdminServices.Util;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PresCrypt_Backend.PresCrypt.API.Controllers;
using PresCrypt_Backend.PresCrypt.Application.Services.AuthServices;
using PresCrypt_Backend.PresCrypt.Application.Services.DoctorServices;
using PresCrypt_Backend.PresCrypt.Application.Services.AppointmentServices;
using PresCrypt_Backend.PresCrypt.Application.Services.PatientServices;
using PresCrypt_Backend.PresCrypt.Application.Services.EmailServices;
using PresCrypt_Backend.PresCrypt.Application.Services.EmailServices.Impl;
using PresCrypt_Backend.PresCrypt.Application.Services.DoctorPatientServices;
using PresCrypt_Backend.PresCrypt.Application.Services.PatientServices.PatientPDFServices;
using PresCrypt_Backend.PresCrypt.API.Hubs;
using PresCrypt_Backend.PresCrypt.Application.Services.EmailServices.PatientEmailServices;
using PresCrypt_Backend.PresCrypt.Application.Services.UserServices;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.SignalR;
using PresCrypt_Backend.PresCrypt.Application.Services.HospitalServices;
using PresCrypt_Backend.PresCrypt.Application.Services.PaymentServices;


var builder = WebApplication.CreateBuilder(args);

// Correct the base path to the folder where appsettings.json is located
builder.Configuration
    .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "PresCrypt.API"))
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLogging(config =>
{
    config.AddConsole();
});


// Register services
builder.Services.AddScoped<IDoctorService, DoctorServices>();
builder.Services.AddScoped<IAdminDoctorService, AdminDoctorService>();
builder.Services.AddScoped<IAdminDoctorRequestService, AdminDoctorRequestService>();
builder.Services.AddTransient<IAdminEmailService, AdminEmailService>();
builder.Services.AddScoped<AdminDoctorUtil>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IDoctorPatientService, DoctorPatientService>();
//builder.Services.AddScoped<IDoctorPrescriptionSubmitService, DoctorPrescriptionSubmitService>();
builder.Services.AddScoped<IAdminPatientService, AdminPatientService>();
builder.Services.AddScoped<IPatientEmailService, PatientEmailService>();
builder.Services.AddScoped<IDoctorDashboardService, DoctorDashboardService>();
builder.Services.AddScoped<DoctorReportService>();
builder.Services.AddScoped<IPDFService, PDFService>();
builder.Services.AddScoped<IHospitalService, HospitalService>();
builder.Services.AddSingleton<IUserIdProvider, QueryStringPatientIdProvider>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IJwtService, JwtService>(); // Scoped registration for JwtService



// Configure JWT Authentication
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.RequireHttpsMetadata = false; // Set to true in production
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true, 
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            ClockSkew = TimeSpan.Zero
        };
    });


// Register controllers for Dependency Injection
builder.Services.AddScoped<PatientController>();
builder.Services.AddScoped<DoctorController>();

var connction = builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"Connection string: {connectionString}");

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000") 
                  .AllowAnyMethod() 
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
});


var app = builder.Build();


// Apply CORS middleware
app.UseCors("AllowReactApp");


// Enable Swagger in Development environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware pipeline setup
app.UseHttpsRedirection();
app.UseRouting();                // 🟢 First, define routing
app.UseCors("AllowReactApp");
app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.MapHub<PatientNotificationHub>("/patientNotificationHub");


app.UseAuthentication(); // Authentication should come before Routing
app.UseAuthorization(); // Authorization after authentication
app.UseRouting(); // Routing middleware after auth
app.MapControllers(); // Map Controllers to Routes
app.Run();
