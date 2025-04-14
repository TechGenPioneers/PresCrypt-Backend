
using Microsoft.EntityFrameworkCore;
using PresCrypt_Backend.PresCrypt.Application.Services.AdminServices;
using PresCrypt_Backend.PresCrypt.Application.Services.AdminServices.Impl;
using PresCrypt_Backend.PresCrypt.Application.Services.AdminServices.Util;
﻿using Microsoft.EntityFrameworkCore;
using PresCrypt_Backend.PresCrypt.Application.Services.DoctorServices;
using PresCrypt_Backend.PresCrypt.Application.Services.AppointmentServices;
using PresCrypt_Backend.PresCrypt.Application.Services.EmailServices;
using PresCrypt_Backend.PresCrypt.Application.Services.EmailServices.Impl;
using PresCrypt_Backend.PresCrypt.Application.Services.DoctorPatientServices;
using PresCrypt_Backend.PresCrypt.Application.Services.DoctorPrescriptionServices;

var builder = WebApplication.CreateBuilder(args);

// Correct the base path to the folder where appsettings.json is located
builder.Configuration
    .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "PresCrypt.API"))
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IDoctorService, DoctorServices>();
builder.Services.AddScoped<IAdminDoctorService, AdminDoctorService>();
builder.Services.AddScoped<IAdminDoctorRequestService, AdminDoctorRequestService>();
builder.Services.AddTransient<IAdminEmailService, AdminEmailService>();
builder.Services.AddScoped<AdminDoctorUtil>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IDoctorPatientService, DoctorPatientService>();
builder.Services.AddScoped<IDoctorPrescriptionSubmitService, DoctorPrescriptionSubmitService>();
builder.Services.AddScoped<IAdminPatientService, AdminPatientService>();

builder.Services.AddHttpClient();

var connction = builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();  // ✅ Allow credentials if needed
        });
});


// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000") 
                  .AllowAnyMethod() 
                  .AllowAnyHeader();
        });
});

var app = builder.Build();


// Apply CORS middleware
app.UseCors("AllowLocalhost3000");


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowReactApp");
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

app.Run();
