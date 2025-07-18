using Microsoft.EntityFrameworkCore;
using PresCrypt_Backend.PresCrypt.Core.Models;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Application.Services.DoctorPatientServices;
using PresCrypt_Backend.PresCrypt.Application.Services.EmailServices.PatientEmailServices;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PresCrypt_Backend.PresCrypt.API.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace PresCrypt_Backend.PresCrypt.Application.Services.AppointmentServices
{
    public class AppointmentService : IAppointmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHubContext<DoctorNotificationHub> _hubContext;
        private readonly IDoctorNotificationService _doctorNotificationService;
        private readonly IPatientEmailService _patientEmailService;
        private readonly ILogger<AppointmentService> _logger;

        public AppointmentService(ApplicationDbContext context, 
            IConfiguration configuration, 
            IHubContext<DoctorNotificationHub> hubContext, 
            IDoctorNotificationService doctorNotificationService,
            IPatientEmailService patientEmailService,
            ILogger<AppointmentService> logger)
        {
            _context = context;
            _hubContext = hubContext;
            _doctorNotificationService = doctorNotificationService;
            _patientEmailService = patientEmailService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<IEnumerable<AppointmentDisplayDto>> GetAppointmentsAsync(string doctorId, DateOnly? date = null)
        {
           
            var currentDate = DateOnly.FromDateTime(DateTime.Now);

            var query = _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.Hospital)
                .Where(a => a.DoctorId == doctorId);

            // Apply date filter if specified, otherwise get future appointments
            query = date.HasValue
                ? query.Where(a => a.Date == date.Value)
                : query.Where(a => a.Date >= currentDate);

            // Execute and map to DTO
            return await query
                .OrderBy(a => a.Date)
                .ThenBy(a => a.Time)
                .Select(a => new AppointmentDisplayDto
                {
                    AppointmentId = a.AppointmentId,
                    Date = a.Date,  
                    Time = a.Time, 
                    Status = a.Status,
                    PatientId = a.Patient.PatientId,
                    HospitalId = a.HospitalId,
                    HospitalName = a.Hospital.HospitalName,
                    PatientName = $"{a.Patient.FirstName} {a.Patient.LastName}",
                    Gender = a.Patient.Gender,
                    DOB = a.Patient.DOB, 
                    ProfileImage = a.Patient.ProfileImage
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<AvailabilityDisplayDto>> GetAvailabilityByDateAsync(string date, string doctorId)
        {
            // Validate date format
            if (!DateTime.TryParse(date, out var parsedDate))
            {
                throw new ArgumentException("Invalid date format. Use YYYY-MM-DD.");
            }

            // Get day of week (e.g., "Monday")
            var dayOfWeek = parsedDate.DayOfWeek.ToString();

            // Query availability with hospital information
            var availability = await _context.DoctorAvailability
                .Include(a => a.Hospital) // Include the Hospital navigation property
                .Where(a => a.DoctorId == doctorId && a.AvailableDay == dayOfWeek)
                .Select(a => new AvailabilityDisplayDto
                {
                    AvailabilityId = a.AvailabilityId,
                    DoctorId = a.DoctorId,
                    AvailableDay = a.AvailableDay,
                    AvailableStartTime = a.AvailableStartTime,
                    AvailableEndTime = a.AvailableEndTime,
                    HospitalId = a.HospitalId,
                    HospitalName = a.Hospital.HospitalName
                })
                .ToListAsync();

            return availability;
        }

        public async Task<Appointment> CreateAppointmentAsync(AppointmentSave dto)
        {
            // Validate the appointment
            var doctorExists = await _context.Doctor.AnyAsync(d => d.DoctorId == dto.DoctorId);
            if (!doctorExists)
            {
                throw new Exception("Doctor not found");
            }

            var patient = await _context.Patient
                .Where(p => p.PatientId == dto.PatientId)
                .Select(p => new { p.PatientId, p.FirstName, p.LastName, p.Email })
                .FirstOrDefaultAsync();

            if (patient == null)
            {
                throw new Exception("Patient not found");
            }

            // Create the appointment
            var appointment = dto.Adapt<Appointment>();
            appointment.CreatedAt = DateTime.UtcNow;
            appointment.AppointmentId = Guid.NewGuid().ToString();

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // Prepare notification details
            var patientFullName = $"{patient.FirstName} {patient.LastName}";
            var message = $"{patient.PatientId} {patientFullName} has booked an appointment on {appointment.Date:MMMM dd, yyyy} at {appointment.Time:h:mm tt}";

            try
            {
                //call the notification service
                await _doctorNotificationService.CreateAndSendNotificationAsync(
                    dto.DoctorId,
                    message,
                    "New Appointment Booked",
                    "Appointment"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending or saving notification for appointment {AppointmentId}", appointment.AppointmentId);
            }

            return appointment;
        }

        public async Task CancelAppointmentAsync(string appointmentId, string patientId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId
                                      && a.PatientId == patientId);

            if (appointment == null)
                throw new Exception("Appointment not found or not owned by patient");

            if (appointment.Status == "Cancelled")
                return;

            appointment.Status = "Cancelled";
            appointment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            try
            {
                // Notify doctor with patient details first
                var doctorMessage = $"Patient ID: {appointment.PatientId}\n" +
                                  $"Name: {appointment.Patient.FirstName} {appointment.Patient.LastName}\n" +
                                  $"Cancelled appointment on {appointment.Date:MMMM dd, yyyy} at {appointment.Time:h:mm tt}\n" +
                                  $"Original appointment ID: {appointmentId}";

                await _doctorNotificationService.CreateAndSendNotificationAsync(
                    appointment.DoctorId,
                    doctorMessage,
                    "APPOINTMENT CANCELLED",
                    "Appointment"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending cancellation notifications for appointment {AppointmentId}", appointmentId);
                // Notification failure shouldn't fail the cancellation
            }
        }

        public async Task<IEnumerable<AppointmentDisplayDto>> GetRecentAppointmentsByDoctorAsync(string doctorId)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            // get the IDs of the most recent PAST appointments per patient
            var recentAppointmentIds = await _context.Appointments
                .Where(a => a.DoctorId == doctorId && a.Date < today) // Only past appointments
                .GroupBy(a => a.PatientId)
                .Select(g => g.OrderByDescending(a => a.Date)
                             .ThenByDescending(a => a.Time)
                             .Select(a => a.AppointmentId)
                             .FirstOrDefault())
                .Where(id => id != null) // Filter out nulls for patients without past appointments
                .ToListAsync();

            // If no past appointments found, return empty list
            if (!recentAppointmentIds.Any())
                return new List<AppointmentDisplayDto>();

            // Then fetch the complete appointment data with includes
            return await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Hospital)
                .Where(a => recentAppointmentIds.Contains(a.AppointmentId))
                .Select(a => new AppointmentDisplayDto
                {
                    AppointmentId = a.AppointmentId,
                    Date = a.Date,
                    Time = a.Time,
                    Status = a.Status,
                    PatientId = a.Patient.PatientId,
                    HospitalId = a.HospitalId,
                    HospitalName = a.Hospital.HospitalName,
                    PatientName = $"{a.Patient.FirstName} {a.Patient.LastName}",
                    Gender = a.Patient.Gender,
                    DOB = a.Patient.DOB,
                    ProfileImage = a.Patient.ProfileImage
                })
                .ToListAsync();
        }

        public async Task<Dictionary<DateTime, int>> GetAppointmentCountsAsync(string doctorId, List<DateTime> dates)
        {
            var result = new Dictionary<DateTime, int>();

            foreach (var date in dates)
            {
                var count = await _context.Appointments
                    .Where(a => a.DoctorId == doctorId && a.Date == DateOnly.FromDateTime(date))
                    .CountAsync();

                result[date] = count;
            }

            return result;
        }


        public async Task<List<PatientAppointmentListDto>> GetAppointmentsByPatientIdAsync(string patientId)
        {
            return await _context.Appointments
                .Where(a => a.PatientId == patientId)
                .Include(a => a.Doctor)
                .Include(a => a.Hospital)
                .Select(a => new PatientAppointmentListDto
                {
                    AppointmentId = a.AppointmentId,
                    PatientName = a.Patient.FirstName + " " + a.Patient.LastName,
                    PatientEmail = a.Patient.Email,
                    DoctorName = a.Doctor.FirstName + " " + a.Doctor.LastName,
                    DoctorEmail = a.Doctor.Email,
                    Specialization = a.Doctor.Specialization,
                    HospitalName = a.Hospital.HospitalName,
                    Time = a.Time,
                    Date = a.Date,
                    Status = a.Status
                })
                .ToListAsync();
        }

        public async Task<CancelAppointmentResultDto> PatientCancelAppointmentAsync(string appointmentId)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);

            if (appointment == null)
                return new CancelAppointmentResultDto { Success = false };

            appointment.Status = "Cancelled";
            appointment.UpdatedAt = DateTime.UtcNow;

            string? paymentMethod = null;
            double paymentAmount = 0;
            string? payHereObjectId = null;

            if (!string.IsNullOrEmpty(appointment.PaymentId))
            {
                var payment = await _context.Payments.FindAsync(appointment.PaymentId);
                if (payment != null)
                {
                    paymentMethod = payment.PaymentMethod;
                    paymentAmount = payment.PaymentAmount;
                    payHereObjectId = payment.PayHereObjectId;

                    if (payment.PaymentMethod == "Card" || payment.PaymentMethod == "Location")
                    {
                        payment.IsRefunded = true;
                        _context.Payments.Update(payment);
                    }
                }
            }

            string? email = null;
            if (!string.IsNullOrEmpty(appointment.PatientId))
            {
                var patient = await _context.Patient.FindAsync(appointment.PatientId);
                if (patient != null)
                {
                    email = patient.Email;
                }
            }

            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();

            return new CancelAppointmentResultDto
            {
                Success = true,
                PaymentMethod = paymentMethod,
                AppointmentDate = appointment.Date,
                AppointmentTime = appointment.Time,
                Email = email,
                PaymentAmount = paymentAmount,
                PayHereObjectId = payHereObjectId
            };
        }




        public async Task<List<AppointmentRescheduleDto>> GetAvailableHospitalsByDateAsync(DateTime date, string doctorId)
        {
            var dayOfWeek = date.DayOfWeek.ToString();


            var hospitals = await _context.DoctorAvailability
                .Include(a => a.Hospital)
                .Where(a => a.AvailableDay == dayOfWeek && a.DoctorId == doctorId)
                .Select(a => new AppointmentRescheduleDto
                {
                    HospitalId = a.Hospital.HospitalId,
                    HospitalName = a.Hospital.HospitalName
                })
                .Distinct()
                .ToListAsync();
            return hospitals;
        }

        public async Task<List<AppointmentRescheduleResultDto>> RescheduleAppointmentsAsync(List<string> appointmentIds)
        {
            var results = new List<AppointmentRescheduleResultDto>();

            // Fetch all eligible appointments with patient information
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.Hospital)
                .Where(a => appointmentIds.Contains(a.AppointmentId) &&
                           a.Status != "Confirmed" &&
                           a.Status != "Rescheduled" &&
                           a.Status != "Completed")
                .ToListAsync();

            foreach (var appointment in appointments)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    _logger.LogInformation("Starting reschedule for appointment {AppointmentId}", appointment.AppointmentId);

                    var nextSlot = await GetNextAvailableSlotAsync(
                        appointment.DoctorId,
                        appointment.HospitalId,
                        appointment.Date.ToDateTime(appointment.Time));

                    if (nextSlot == null)
                    {
                        _logger.LogWarning("No available slot found for appointment {AppointmentId}", appointment.AppointmentId);
                        results.Add(new AppointmentRescheduleResultDto
                        {
                            Success = false,
                            OriginalAppointmentId = appointment.AppointmentId,
                            Message = "No available slots found for rescheduling."
                        });
                        continue;
                    }

                    // Update original appointment
                    appointment.Status = "Rescheduled";
                    appointment.UpdatedAt = DateTime.UtcNow;

                    // Create new appointment
                    var newAppointment = new Appointment
                    {
                        AppointmentId = Guid.NewGuid().ToString(),
                        PatientId = appointment.PatientId,
                        DoctorId = appointment.DoctorId,
                        HospitalId = appointment.HospitalId,
                        Date = DateOnly.FromDateTime(nextSlot.Value),
                        Time = TimeOnly.FromDateTime(nextSlot.Value),
                        Charge = appointment.Charge,
                        Status = "Pending Confirmation",
                        TypeOfAppointment = appointment.TypeOfAppointment,
                        CreatedAt = DateTime.UtcNow,
                        SpecialNote = $"Auto rescheduled from {appointment.AppointmentId}"
                    };

                    _context.Appointments.Add(newAppointment);
                    await _context.SaveChangesAsync();

                    // Email sending with detailed logging
                    if (!string.IsNullOrEmpty(appointment.Patient?.Email))
                    {
                        try
                        {
                            _logger.LogInformation("Preparing to send email for new appointment {NewAppointmentId}",
                                newAppointment.AppointmentId);

                            var originalAppointmentDateTime = appointment.Date.ToDateTime(appointment.Time);
                            var rescheduledDateTime = nextSlot.Value;

                            var emailDto = new AppointmentRescheduleEmailDto
                            {
                                Email = appointment.Patient.Email,
                                Name = $"{appointment.Patient.FirstName} {appointment.Patient.LastName}",
                                AppointmentId = newAppointment.AppointmentId,
                                OldDateTime = originalAppointmentDateTime, // The original appointment date/time
                                NewDateTime = rescheduledDateTime,
                            };

                            _logger.LogDebug("Email details: {@EmailDto}", emailDto);

                            await _patientEmailService.SendRescheduleConfirmationEmailAsync(emailDto);

                            _logger.LogInformation("Email sent successfully for appointment {AppointmentId}",
                                newAppointment.AppointmentId);
                        }
                        catch (Exception emailEx)
                        {
                            _logger.LogError(emailEx,
                                "Failed to send email for rescheduled appointment {AppointmentId}. Error: {ErrorMessage}",
                                newAppointment.AppointmentId, emailEx.Message);

                            // Add email failure information to the result
                            results.Add(new AppointmentRescheduleResultDto
                            {
                                Success = true, // Reschedule succeeded, email failed
                                OriginalAppointmentId = appointment.AppointmentId,
                                NewAppointmentId = newAppointment.AppointmentId,
                                NewDateTime = nextSlot.Value,
                                Message = $"Successfully rescheduled but email failed: {emailEx.Message}"
                            });

                            await transaction.CommitAsync();
                            continue;
                        }
                    }
                    else
                    {
                        _logger.LogWarning("No email address found for patient {PatientId}", appointment.PatientId);
                    }

                    await transaction.CommitAsync();

                    results.Add(new AppointmentRescheduleResultDto
                    {
                        Success = true,
                        OriginalAppointmentId = appointment.AppointmentId,
                        NewAppointmentId = newAppointment.AppointmentId,
                        NewDateTime = nextSlot.Value,
                        Message = $"Successfully rescheduled to {nextSlot.Value:MMMM dd, yyyy hh:mm tt}"
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex,
                        "Failed to reschedule appointment {AppointmentId}. Error: {ErrorMessage}",
                        appointment.AppointmentId, ex.Message);

                    results.Add(new AppointmentRescheduleResultDto
                    {
                        Success = false,
                        OriginalAppointmentId = appointment.AppointmentId,
                        Message = $"Failed to reschedule: {ex.Message}"
                    });
                }
            }

            return results;
        }

        public async Task<DateTime?> GetNextAvailableSlotAsync(string doctorId, string hospitalId, DateTime afterDate)
        {
            var availabilities = await _context.DoctorAvailability
                .Where(a => a.DoctorId == doctorId && a.HospitalId == hospitalId)
                .ToListAsync();

            var checkDate = afterDate.Date.AddDays(1); // start from next day

            for (int i = 0; i < 30; i++) // Check up to 30 days in advance
            {
                var currentDate = checkDate.AddDays(i);
                var currentDayOfWeek = currentDate.DayOfWeek;

                foreach (var availability in availabilities)
                {
                    var availableDayOfWeek = Enum.Parse<DayOfWeek>(availability.AvailableDay);
                    if (availableDayOfWeek != currentDayOfWeek)
                        continue;

                    var currentDateOnly = DateOnly.FromDateTime(currentDate);

                    for (var time = availability.AvailableStartTime;
                        time < availability.AvailableEndTime;
                        time = time.AddMinutes(30))
                    {
                        bool isBooked = await _context.Appointments.AnyAsync(a =>
                            a.DoctorId == doctorId &&
                            a.HospitalId == hospitalId &&
                            a.Date == currentDateOnly &&
                            a.Time == time &&
                            a.Status != "Cancelled" &&
                            a.Status != "Completed");

                        if (!isBooked)
                        {
                            return new DateTime(
                                currentDate.Year,
                                currentDate.Month,
                                currentDate.Day,
                                time.Hour,
                                time.Minute,
                                0);
                        }
                    }
                }
            }

            return null;
        }

        private AppointmentRescheduleResultDto CreateFailureResult(string message)
        {
            return new AppointmentRescheduleResultDto
            {
                Success = false,
                Message = message
            };
        }
        public async Task<List<PatientAppointmentListDto>> GetAppointmentsByDateRangeAsync(DateOnly startDate, DateOnly endDate, string? patientId)
        {
            var query = _context.Appointments
                .Where(a => a.Date >= startDate && a.Date <= endDate);

            if (!string.IsNullOrWhiteSpace(patientId))
            {
                query = query.Where(a => a.PatientId == patientId);
            }

            return await query
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.Hospital)
                .Select(a => new PatientAppointmentListDto
                {
                    AppointmentId = a.AppointmentId,
                    PatientName = a.Patient.FirstName + " " + a.Patient.LastName,
                    PatientEmail = a.Patient.Email,
                    DoctorName = a.Doctor.FirstName + " " + a.Doctor.LastName,
                    DoctorEmail = a.Doctor.Email,
                    Specialization = a.Doctor.Specialization,
                    HospitalName = a.Hospital.HospitalName,
                    Time = a.Time,
                    Date = a.Date,
                    Status = a.Status
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<AppointmentViewDialogDto>> GetAppointmentsByPatientIdAndDateAsync(string patientId, DateTime date)
        {
            return await _context.Appointments
                .Where(a => a.PatientId == patientId && a.Date == DateOnly.FromDateTime(date))
                .Include(a => a.Doctor)
                .Include(a => a.Hospital)
                .Select(a => new AppointmentViewDialogDto
                {
                    AppointmentId = a.AppointmentId,
                    DoctorName = a.Doctor.FirstName + " " +  a.Doctor.LastName,
                    Specialization = a.Doctor.Specialization,
                    HospitalName = a.Hospital.HospitalName,
                    AppointmentTime = a.Time.ToString("HH:mm"),
                    Status = a.Status,
                    AppointmentDate = a.Date 
                })
                .ToListAsync();
        }




    }
}