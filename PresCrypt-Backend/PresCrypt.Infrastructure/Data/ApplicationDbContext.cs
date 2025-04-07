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
    public DbSet<Patient> Patient { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<Hospital> Hospitals { get; set; }
<<<<<<< HEAD
    public DbSet<DoctorAvailability> DoctorAvailability { get; set; } // This is the correct declaration

=======
    public DbSet<DoctorAvailability> DoctorAvailability { get; set; } 
>>>>>>> b35b88d376ae6d4f7c5ad59480e5fc70b96a5627
}
