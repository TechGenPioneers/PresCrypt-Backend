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
    public DbSet<PatientNotifications> PatientNotifications { get; set; }
    public DbSet<DoctorNotification> DoctorNotifications { get; set; }
    public DbSet<AdminNotification> AdminNotifications { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<DoctorPatientAccessRequest> DoctorPatientAccessRequests { get; set; }

    public DbSet<PatientContactUs> PatientContactUs { get; set; }


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
            .HasPrincipalKey(u => u.UserName)  // Link to UserName
            .OnDelete(DeleteBehavior.Restrict);

        // Doctor.Email -> User.UserName
        modelBuilder.Entity<Doctor>()
            .HasOne(d => d.User)
            .WithMany(u => u.Doctor)
            .HasForeignKey(d => d.Email)
            .HasPrincipalKey(u => u.UserName)  // Link to UserName
            .OnDelete(DeleteBehavior.Restrict);

        // Admin.Email -> User.UserName
        modelBuilder.Entity<Admin>()
            .HasOne(a => a.User)
            .WithMany(u => u.Admin)
            .HasForeignKey(a => a.Email)
            .HasPrincipalKey(u => u.UserName)  // Link to UserName
            .OnDelete(DeleteBehavior.Restrict);


        // Cascade Delete
        modelBuilder.Entity<AdminNotification>()
            .HasOne(an => an.Doctor)
            .WithMany()
            .HasForeignKey(an => an.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Cascade Delete
        modelBuilder.Entity<AdminNotification>()
            .HasOne(an => an.Patient)
            .WithMany()
            .HasForeignKey(an => an.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        // Cascade Delete
        modelBuilder.Entity<AdminNotification>()
             .HasOne(an => an.DoctorRequest)
             .WithMany()
             .HasForeignKey(an => an.RequestId)
             .OnDelete(DeleteBehavior.Cascade);

    }
}


