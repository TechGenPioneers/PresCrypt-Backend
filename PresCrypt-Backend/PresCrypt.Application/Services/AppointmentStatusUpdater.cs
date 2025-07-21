using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class AppointmentStatusUpdater : IHostedService
{
    private readonly IServiceProvider _services;
    private Timer _timer;

    public AppointmentStatusUpdater(IServiceProvider services)
    {
        _services = services;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Run once immediately, then every 24 hours
        _timer = new Timer(UpdateAppointments, null, TimeSpan.Zero, TimeSpan.FromMinutes(2));
        return Task.CompletedTask;
    }

    private void UpdateAppointments(object state)
    {
        using (var scope = _services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var today = DateOnly.FromDateTime(DateTime.Today);

            var appointmentsToCancel = db.Appointments
                .Where(a => a.Status == "Pending" && a.Date < today)
                .ToList();

            foreach (var appt in appointmentsToCancel)
            {
                appt.Status = "Cancelled";
            }

            db.SaveChanges();
        }
    }


    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Dispose();
        return Task.CompletedTask;
    }
}
