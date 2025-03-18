using PresCrypt_Backend.PresCrypt.Core.Models;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets for your entities
    public DbSet<Doctor> Doctor { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<Hospital> Hospitals { get; set; }
    public DbSet<DoctorAvailability> Doctor_Availability { get; set; } // This is the correct declaration

    public DbSet<HospitalDoctor> HospitalDoctor { get; set; }
}
