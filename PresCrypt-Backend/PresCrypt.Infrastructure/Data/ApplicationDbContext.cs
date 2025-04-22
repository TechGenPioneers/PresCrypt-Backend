using PresCrypt_Backend.PresCrypt.Core.Models;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets for your entities
    public DbSet<User> User { get; set; }
    public DbSet<Doctor> Doctor { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<Hospital> Hospitals { get; set; }
    public DbSet<DoctorAvailability> DoctorAvailability { get; set; }
    public DbSet<Patient> Patient { get; set; }
    public DbSet<Admin> Admin { get; set; }
    public DbSet<DoctorRequest> DoctorRequest { get; set; }
    public DbSet<RequestAvailability> RequestAvailability { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // UserName should be unique
        modelBuilder.Entity<User>()
            .HasIndex(u => u.UserName)
            .IsUnique();

        // Patient.Email -> User.UserName
        modelBuilder.Entity<Patient>()
            .HasOne(p => p.User)
            .WithMany(u => u.Patient)
            .HasForeignKey(p => p.Email)
            .HasPrincipalKey(u => u.UserName)  // Link to UserName, NOT UserId
            .OnDelete(DeleteBehavior.Restrict);

        // Doctor.Email -> User.UserName
        modelBuilder.Entity<Doctor>()
            .HasOne(d => d.User)
            .WithMany(u => u.Doctor)
            .HasForeignKey(d => d.Email)
            .HasPrincipalKey(u => u.UserName)  // Link to UserName, NOT UserId
            .OnDelete(DeleteBehavior.Restrict);

        // Admin.Email -> User.UserName
        modelBuilder.Entity<Admin>()
            .HasOne(a => a.User)
            .WithMany(u => u.Admin)
            .HasForeignKey(a => a.Email)
            .HasPrincipalKey(u => u.UserName)  // Link to UserName, NOT UserId
            .OnDelete(DeleteBehavior.Restrict);

        //modelBuilder.Entity<DoctorRequest>()
        //    .HasMany(dr => dr.RequestAvailability)
        //    .WithOne(ra => ra.DoctorRequest)
        //    .HasForeignKey(ra => ra.DoctorRequestId);
            

        // Hospital -> RequestAvailability (One-to-Many)
        //modelBuilder.Entity<Hospital>()
        //    .HasMany(h => h.RequestAvailability)
        //    .WithOne(ra => ra.Hospital)
        //    .HasForeignKey(ra => ra.HospitalId)
        //    .OnDelete(DeleteBehavior.Restrict);
        //modelBuilder.Entity<RequestAvailability>()
        //    .Property(ra => ra.AvailableStartTime)
        //    .HasConversion(
        //        v => v.ToTimeSpan(),
        //        v => TimeOnly.FromTimeSpan(v));

        //modelBuilder.Entity<RequestAvailability>()
        //    .Property(ra => ra.AvailableEndTime)
        //    .HasConversion(
        //        v => v.ToTimeSpan(),
        //        v => TimeOnly.FromTimeSpan(v));

        //modelBuilder.Entity<DoctorAvailability>()
        //    .Property(da => da.AvailableStartTime)
        //    .HasConversion(
        //        v => v.ToTimeSpan(),
        //        v => TimeOnly.FromTimeSpan(v));

        //modelBuilder.Entity<DoctorAvailability>()
        //    .Property(da => da.AvailableEndTime)
        //    .HasConversion(
        //        v => v.ToTimeSpan(),
        //        v => TimeOnly.FromTimeSpan(v));



    }
}


