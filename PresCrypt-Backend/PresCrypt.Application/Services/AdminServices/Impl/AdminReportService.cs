using Microsoft.EntityFrameworkCore;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Core.Models;

namespace PresCrypt_Backend.PresCrypt.Application.Services.AdminServices.Impl
{
    public class AdminReportService : IAdminReportService
    {
        private readonly ApplicationDbContext _context;
        public AdminReportService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<AdminReportAllDto> GetAllDetails()
        {
            try
            {
                var patients = await _context.Patient
              .Select(d => new AdminReportPatientsDto
              {
                  PatientId = d.PatientId,
                  FirstName = d.FirstName,
                  LastName = d.LastName
              })
              .ToListAsync();

                var doctors = await _context.Doctor
                  .Select(d => new AdminReportDoctorsDto
                  {
                      DoctorId = d.DoctorId,
                      FirstName = d.FirstName,
                      LastName = d.LastName
                  })
                  .ToListAsync();

                var specialties = await _context.Doctor
                     .Select(d => d.Specialization)
                     .Distinct()
        .ToListAsync();

                var ReportDetails = new AdminReportAllDto()
                {
                    Patients = patients,
                    Doctors = doctors,
                    Specialty=specialties

                };
                return ReportDetails;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<AdminReportDetailsDto> GetFilteredDetails(AdminGetReportDetailsDto reportDetails)
        {
            var adminReportDetails = new AdminReportDetailsDto();
            if(reportDetails.ReportType == "summary" || reportDetails.ReportType == "detailed")
            {
                if (reportDetails.Patient != "")
                {


                    if (reportDetails.Patient == "all")
                    {
                        DateOnly? fromDate = null;
                        DateOnly? toDate = null;

                        if (!string.IsNullOrEmpty(reportDetails.FromDate))
                        {
                            var parsedFromDate = DateTime.Parse(reportDetails.FromDate);
                            fromDate = DateOnly.FromDateTime(parsedFromDate);
                        }

                        if (!string.IsNullOrEmpty(reportDetails.ToDate))
                        {
                            var parsedToDate = DateTime.Parse(reportDetails.ToDate);
                            toDate = DateOnly.FromDateTime(parsedToDate);
                        }

                        // Start query
                        var query = _context.Patient.AsQueryable();

                        // Apply flexible filtering
                        if (fromDate.HasValue)
                        {
                            query = query.Where(d => DateOnly.FromDateTime(d.CreatedAt) >= fromDate.Value);
                        }

                        if (toDate.HasValue)
                        {
                            query = query.Where(d => DateOnly.FromDateTime(d.CreatedAt) <= toDate.Value);
                        }

                        // Now select
                        var patients = await query
                            .Select(d => new AdminPatientDto
                            {
                                PatientId = d.PatientId,
                                FirstName = d.FirstName,
                                LastName = d.LastName,
                                DOB = d.DOB.ToString("yyyy-MM-dd"),
                                Gender = d.Gender,
                                Email = d.Email,
                                NIC = d.NIC,
                                ProfileImage = d.ProfileImage,
                                CreatedAt = d.CreatedAt.ToString("yyyy-MM-dd"),
                                UpdatedAt = d.UpdatedAt.ToString("yyyy-MM-dd"),
                                Status = d.Status,
                                LastLogin = d.LastLogin.HasValue ? d.LastLogin.Value.ToString("yyyy-MM-dd HH:mm:ss") : null
                            })
                            .ToListAsync();

                        adminReportDetails.PatientList = patients;
                    }
                    else
                    {
                        var patients = await _context.Patient
                            .Where(d => d.PatientId == reportDetails.Patient)
                            .Select(d => new AdminPatientDto
                            {
                                PatientId = d.PatientId,
                                FirstName = d.FirstName,
                                LastName = d.LastName,
                                DOB = d.DOB.ToString("yyyy-MM-dd"),
                                Gender =d.Gender,
                                Email = d.Email,
                                NIC = d.NIC,
                                ProfileImage = d.ProfileImage,
                                CreatedAt = d.CreatedAt.ToString("yyyy-MM-dd"),
                                UpdatedAt = d.UpdatedAt.ToString("yyyy-MM-dd"),
                                Status = d.Status,
                                LastLogin = d.LastLogin.HasValue ? d.LastLogin.Value.ToString("yyyy-MM-dd HH:mm:ss") : null
                            })
                            .FirstOrDefaultAsync();
                        adminReportDetails.SinglePatient=patients;
                    }


                    return adminReportDetails;

                }
                else if (reportDetails.Doctor != "")
                {

                    if (reportDetails.Doctor == "all")
                    {
                        DateOnly? fromDate = null;
                        DateOnly? toDate = null;

                        if (!string.IsNullOrEmpty(reportDetails.FromDate))
                        {
                            var parsedFromDate = DateTime.Parse(reportDetails.FromDate);
                            fromDate = DateOnly.FromDateTime(parsedFromDate);
                        }

                        if (!string.IsNullOrEmpty(reportDetails.ToDate))
                        {
                            var parsedToDate = DateTime.Parse(reportDetails.ToDate);
                            toDate = DateOnly.FromDateTime(parsedToDate);
                        }

                        // Start query
                        var query = _context.Doctor.AsQueryable();

                        // Apply flexible filtering
                        if (fromDate.HasValue)
                        {
                            query = query.Where(d => DateOnly.FromDateTime(d.CreatedAt) >= fromDate.Value);
                        }

                        if (toDate.HasValue)
                        {
                            query = query.Where(d => DateOnly.FromDateTime(d.CreatedAt) <= toDate.Value);
                        }

                        // Now select
                        var doctors = await query
                            .Select(d => new AdminDoctorDto
                            {
                                DoctorId = d.DoctorId,
                                FirstName = d.FirstName,
                                LastName = d.LastName,
                                Gender = d.Gender,
                                ProfilePhoto = d.DoctorImage,
                                Email = d.Email,
                                Specialization = d.Specialization,
                                SlmcLicense = d.SLMCRegId,
                                NIC = d.NIC,
                                Charge=d.Charge,
                                Description = d.Description,
                                EmailVerified = d.EmailVerified,
                                Status = d.Status,
                                CreatedAt = d.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                                UpdatedAt = d.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                                LastLogin = d.LastLogin.HasValue ? d.LastLogin.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                                ContactNumber = d.ContactNumber
                            })
                            .ToListAsync();

                        adminReportDetails.DoctorList=doctors;
                    }
                    else
                    {
                        var doctor = await _context.Doctor
                            .Where(d => d.DoctorId == reportDetails.Doctor)
                            .Select(d => new AdminDoctorDto
                            {
                                DoctorId = d.DoctorId,
                                FirstName = d.FirstName,
                                LastName = d.LastName,
                                Gender = d.Gender,
                                ProfilePhoto = d.DoctorImage,
                                Email = d.Email,
                                Specialization = d.Specialization,
                                SlmcLicense = d.SLMCRegId,
                                NIC = d.NIC,
                                Charge=d.Charge,
                                Description = d.Description,
                                EmailVerified = d.EmailVerified,
                                Status = d.Status,
                                CreatedAt = d.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                                UpdatedAt = d.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                                LastLogin = d.LastLogin.HasValue ? d.LastLogin.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                                ContactNumber = d.ContactNumber
                            })
                             .FirstOrDefaultAsync();
                        if (doctor != null)
                        {

                            // Fetch doctor availability along with hospital names
                            var doctorAvailability = await _context.DoctorAvailability
                                .Where(d => d.DoctorId == reportDetails.Doctor)
                                .Join(
                                    _context.Hospitals,
                                    a => a.HospitalId,// Foreign key in Doctor_Availability
                                    h => h.HospitalId,// Primary key in Hospital tablev
                                    (a, h) => new AvailabilityDto
                                    {
                                        AvailabilityId = a.AvailabilityId.ToString(),
                                        Day = a.AvailableDay,
                                        StartTime = a.AvailableStartTime.ToString("HH:mm"),  // Formats as HH:mm
                                        EndTime = a.AvailableEndTime.ToString("HH:mm"),
                                        HospitalName = h.HospitalName,
                                        HospitalId = h.HospitalId
                                    }
                                )
                                .ToListAsync();
                            adminReportDetails.SingleDoctor=doctor;
                            adminReportDetails.Availability=doctorAvailability;
                        }
                    }
                    return adminReportDetails;

                }
                else if(reportDetails.Specialty != "")
                {
                    var doctors = await _context.Doctor
                        .Where(d=> d.Specialization == reportDetails.Specialty)
                       .Select(d => new AdminDoctorDto
                       {
                           DoctorId = d.DoctorId,
                           FirstName = d.FirstName,
                           LastName = d.LastName,
                           Gender = d.Gender,
                           ProfilePhoto = d.DoctorImage,
                           Email = d.Email,
                           Specialization = d.Specialization,
                           SlmcLicense = d.SLMCRegId,
                           NIC = d.NIC,
                           Charge=d.Charge,
                           Description = d.Description,
                           EmailVerified = d.EmailVerified,
                           Status = d.Status,
                           CreatedAt = d.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                           UpdatedAt = d.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                           LastLogin = d.LastLogin.HasValue ? d.LastLogin.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                           ContactNumber = d.ContactNumber
                       })
                          .ToListAsync();
                    adminReportDetails.DoctorList=doctors;
                    return adminReportDetails;
                }
            }
            else if (reportDetails.ReportType == "appointment")
            {
                if (reportDetails.Patient != "" && reportDetails.Doctor != "") 
                {
                    DateOnly? fromDate = null;
                    DateOnly? toDate = null;

                    if (!string.IsNullOrEmpty(reportDetails.FromDate))
                    {
                        var parsedFromDate = DateTime.Parse(reportDetails.FromDate);
                        fromDate = DateOnly.FromDateTime(parsedFromDate);
                    }

                    if (!string.IsNullOrEmpty(reportDetails.ToDate))
                    {
                        var parsedToDate = DateTime.Parse(reportDetails.ToDate);
                        toDate = DateOnly.FromDateTime(parsedToDate);
                    }


                    var appointments = await (
                           from a in _context.Appointments
                           join d in _context.Doctor on a.DoctorId equals d.DoctorId
                           join p in _context.Patient on a.PatientId equals p.PatientId
                           join h in _context.Hospitals on a.HospitalId equals h.HospitalId
                           where a.PatientId == reportDetails.Patient
                           && (a.DoctorId == reportDetails.Doctor)
                            && (!fromDate.HasValue || a.Date >= fromDate)
                            && (!toDate.HasValue || a.Date <= toDate)
                           select new AdminAllAppointmentsDto
                           {
                               AppointmentId = a.AppointmentId,
                               DoctorId = a.DoctorId,
                               DoctorName = d.FirstName + " " + d.LastName,
                               PatientId = p.PatientId,
                               PatientName = p.FirstName + " " + p.LastName,
                               HospitalId = a.HospitalId,
                               HospitalName = h.HospitalName,
                               Date = a.Date.ToString("yyyy-MM-dd"),
                               Time = a.Time.ToString("HH:mm"),
                               Charge = a.Charge,
                               Status = a.Status,
                               SpecialNote = a.SpecialNote,
                               TypeOfAppointment = a.TypeOfAppointment,
                               CreatedAt = a.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                               UpdatedAt = a.UpdatedAt.HasValue ? a.UpdatedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                           })
                           .ToListAsync();

                    adminReportDetails.Appointments=appointments;
                    return adminReportDetails;

                }
                else if (reportDetails.Patient != "" && reportDetails.Specialty != "")
                {
                        DateOnly? fromDate = null;
                        DateOnly? toDate = null;

                        if (!string.IsNullOrEmpty(reportDetails.FromDate))
                        {
                            var parsedFromDate = DateTime.Parse(reportDetails.FromDate);
                            fromDate = DateOnly.FromDateTime(parsedFromDate);
                        }

                        if (!string.IsNullOrEmpty(reportDetails.ToDate))
                        {
                            var parsedToDate = DateTime.Parse(reportDetails.ToDate);
                            toDate = DateOnly.FromDateTime(parsedToDate);
                        }

                        // Filter doctors by their specialization
                        var doctorIds = await _context.Doctor
                            .Where(d => d.Specialization == reportDetails.Specialty)
                            .Select(d => d.DoctorId)
                            .ToListAsync();

                        var appointments = await (
                            from a in _context.Appointments
                            join d in _context.Doctor on a.DoctorId equals d.DoctorId
                            join p in _context.Patient on a.PatientId equals p.PatientId
                            join h in _context.Hospitals on a.HospitalId equals h.HospitalId
                            where a.PatientId == reportDetails.Patient
                            && doctorIds.Contains(a.DoctorId) // Filter appointments by doctorIds
                            && (!fromDate.HasValue || a.Date >= fromDate)
                            && (!toDate.HasValue || a.Date <= toDate)
                            select new AdminAllAppointmentsDto
                            {
                                AppointmentId = a.AppointmentId,
                                DoctorId = a.DoctorId,
                                DoctorName = d.FirstName + " " + d.LastName,
                                PatientId = p.PatientId,
                                PatientName = p.FirstName + " " + p.LastName,
                                HospitalId = a.HospitalId,
                                HospitalName = h.HospitalName,
                                Date = a.Date.ToString("yyyy-MM-dd"),
                                Time = a.Time.ToString("HH:mm"),
                                Charge = a.Charge,
                                Status = a.Status,
                                SpecialNote = a.SpecialNote,
                                TypeOfAppointment = a.TypeOfAppointment,
                                CreatedAt = a.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                                UpdatedAt = a.UpdatedAt.HasValue ? a.UpdatedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                            })
                            .ToListAsync();

                        adminReportDetails.Appointments = appointments;
                        return adminReportDetails;
                    

                }
                else
                {
                    DateOnly? fromDate = null;
                    DateOnly? toDate = null;

                    if (!string.IsNullOrEmpty(reportDetails.FromDate))
                    {
                        var parsedFromDate = DateTime.Parse(reportDetails.FromDate);
                        fromDate = DateOnly.FromDateTime(parsedFromDate);
                    }

                    if (!string.IsNullOrEmpty(reportDetails.ToDate))
                    {
                        var parsedToDate = DateTime.Parse(reportDetails.ToDate);
                        toDate = DateOnly.FromDateTime(parsedToDate);
                    }



                    if (reportDetails.Patient != "")
                    {
                        var appointments = await (
                            from a in _context.Appointments
                            join d in _context.Doctor on a.DoctorId equals d.DoctorId
                            join p in _context.Patient on a.PatientId equals p.PatientId
                            join h in _context.Hospitals on a.HospitalId equals h.HospitalId
                            where a.PatientId == reportDetails.Patient
                             && (!fromDate.HasValue || a.Date >= fromDate)
                             && (!toDate.HasValue || a.Date <= toDate)
                            select new AdminAllAppointmentsDto
                            {
                                AppointmentId = a.AppointmentId,
                                DoctorId = a.DoctorId,
                                DoctorName = d.FirstName + " " + d.LastName,
                                PatientId = p.PatientId,
                                PatientName = p.FirstName + " " + p.LastName,
                                HospitalId = a.HospitalId,
                                HospitalName = h.HospitalName,
                                Date = a.Date.ToString("yyyy-MM-dd"),
                                Time = a.Time.ToString("HH:mm"),
                                Charge = a.Charge,
                                Status = a.Status,
                                SpecialNote = a.SpecialNote,
                                TypeOfAppointment = a.TypeOfAppointment,
                                CreatedAt = a.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                                UpdatedAt = a.UpdatedAt.HasValue ? a.UpdatedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                            })
                            .ToListAsync();

                        adminReportDetails.Appointments=appointments;
                        return adminReportDetails;
                    }
                    else if (reportDetails.Doctor != "")
                    {


                        var appointments = await (
                           from a in _context.Appointments
                           join d in _context.Doctor on a.DoctorId equals d.DoctorId
                           join h in _context.Hospitals on a.HospitalId equals h.HospitalId
                           join p in _context.Patient on a.PatientId equals p.PatientId
                           where a.DoctorId == reportDetails.Doctor
                             && (!fromDate.HasValue || a.Date >= fromDate)
                             && (!toDate.HasValue || a.Date <= toDate)
                           select new AdminAllAppointmentsDto
                           {
                               AppointmentId = a.AppointmentId,
                               DoctorId = a.DoctorId,
                               DoctorName = d.FirstName + " " + d.LastName,
                               PatientId= a.PatientId,
                               PatientName = p.FirstName + " " + p.LastName,
                               HospitalId = a.HospitalId,
                               HospitalName = h.HospitalName,
                               Date = a.Date.ToString("yyyy-MM-dd"),
                               Time = a.Time.ToString("HH:mm"),
                               Charge = a.Charge,
                               Status = a.Status,
                               SpecialNote = a.SpecialNote,
                               TypeOfAppointment = a.TypeOfAppointment,
                               CreatedAt = a.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                               UpdatedAt = a.UpdatedAt.HasValue ? a.UpdatedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                           })
                           .ToListAsync();
                        adminReportDetails.Appointments=appointments;
                        return adminReportDetails;
                    }
                    else if (reportDetails.Specialty != "")
                    {
                        var doctorIds = await _context.Doctor
                            .Where(d => d.Specialization == reportDetails.Specialty)
                            .Select(d => d.DoctorId)
                            .ToListAsync();

                        var appointments = await (
                            from a in _context.Appointments
                            join d in _context.Doctor on a.DoctorId equals d.DoctorId
                            join h in _context.Hospitals on a.HospitalId equals h.HospitalId
                            join p in _context.Patient on a.PatientId equals p.PatientId
                            where doctorIds.Contains(a.DoctorId) // Filter appointments by doctorIds
                             && (!fromDate.HasValue || a.Date >= fromDate)
                             && (!toDate.HasValue || a.Date <= toDate)
                            select new AdminAllAppointmentsDto
                            {
                                AppointmentId = a.AppointmentId,
                                DoctorId = a.DoctorId,
                                DoctorName = d.FirstName + " " + d.LastName,
                                PatientId= a.PatientId,
                                PatientName = p.FirstName + " " + p.LastName,
                                HospitalId = a.HospitalId,
                                HospitalName = h.HospitalName,
                                Date = a.Date.ToString("yyyy-MM-dd"),
                                Time = a.Time.ToString("HH:mm"),
                                Charge = a.Charge,
                                Status = a.Status,
                                SpecialNote = a.SpecialNote,
                                TypeOfAppointment = a.TypeOfAppointment,
                                CreatedAt = a.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                                UpdatedAt = a.UpdatedAt.HasValue ? a.UpdatedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                            })
                            .ToListAsync();
                        adminReportDetails.Appointments=appointments;
                        return adminReportDetails;
                    }
                }
            }
            else if (reportDetails.ReportType == "activity")
            {
                if(reportDetails.Patient != "")
                {
                    var patientActivity = await _context.Patient
                    .Select(patient => new AdminUserActivityDto
                      {
                         UserId = patient.PatientId,
                        UserName = patient.FirstName + " " + patient.LastName,
                        CreatedAt = patient.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                         UpdatedAt = patient.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                          LastLogin = patient.LastLogin.HasValue ? patient.LastLogin.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,

                         // Latest appointment details
                          LastAppointmentCreatedAt = _context.Appointments
                            .Where(a => a.PatientId == patient.PatientId)
                            .OrderByDescending(a => a.CreatedAt) // sort by created date
                            .Select(a => a.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"))
                           .FirstOrDefault(),

                          LastAppointmentStatus = _context.Appointments
                           .Where(a => a.PatientId == patient.PatientId)
                        .OrderByDescending(a => a.CreatedAt)
                        .Select(a => a.Status)
                        .FirstOrDefault(),
                      })
                      .FirstOrDefaultAsync();


                    adminReportDetails.UserActivity=patientActivity;
                    return adminReportDetails;
                }
                if (reportDetails.Doctor != "")
                {
                    var patientActivity = await _context.Doctor
                    .Select(doctor => new AdminUserActivityDto
                    {
                        UserId = doctor.DoctorId,
                        UserName = doctor.FirstName + " " + doctor.LastName,
                        CreatedAt = doctor.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                        UpdatedAt = doctor.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                        LastLogin = doctor.LastLogin.HasValue ? doctor.LastLogin.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,

                        // Latest appointment details
                        LastAppointmentCreatedAt = _context.Appointments
                            .Where(a => a.DoctorId == doctor.DoctorId)
                            .OrderByDescending(a => a.CreatedAt) // sort by created date
                            .Select(a => a.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"))
                           .FirstOrDefault(),

                        LastAppointmentStatus = _context.Appointments
                           .Where(a => a.DoctorId == doctor.DoctorId)
                        .OrderByDescending(a => a.CreatedAt)
                        .Select(a => a.Status)
                        .FirstOrDefault(),
                    })
                      .FirstOrDefaultAsync();


                    adminReportDetails.UserActivity=patientActivity;
                    return adminReportDetails;
                }
            }
            return null;
        }
    }
}
