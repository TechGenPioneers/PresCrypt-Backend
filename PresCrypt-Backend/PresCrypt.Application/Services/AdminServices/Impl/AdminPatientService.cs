using Microsoft.EntityFrameworkCore;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Core.Models;

namespace PresCrypt_Backend.PresCrypt.Application.Services.AdminServices.Impl
{
    public class AdminPatientService : IAdminPatientService
    {
        private readonly ApplicationDbContext _context;
        public AdminPatientService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<AdminAllPatientDto>> GetAllPatients()
        {
            try
            {
                var patients = await _context.Patient
                     .Select(p => new AdminAllPatientDto
                         {
                           PatientId = p.PatientId,
                           FirstName = p.FirstName,
                           LastName = p.LastName,
                          DOB = p.DOB.ToString("yyyy-MM-dd"),
                           Gender = p.Gender,
                               ProfileImage = p.ProfileImage,
                                 Status = p.Status,
                           LastLogin = p.LastLogin.HasValue
                            ? p.LastLogin.Value.ToString("yyyy-MM-dd HH:mm:ss")
                              : null
                        })
                     .ToListAsync();


                return patients;
            }
            catch (Exception)
            {
                throw;
            }
        }



        public async Task<AdminPatientAppointmentsDto> GetPatientById(string patientId)
        {
            try
            {
                var patient = await _context.Patient
                    .Where(d => d.PatientId == patientId)
                    .Select(d => new AdminPatientDto
                    {
                        PatientId = d.PatientId,
                        FirstName = d.FirstName,
                        LastName = d.LastName,
                        DOB = d.DOB.ToString("yyyy-MM-dd"),
                        Gender = d.Gender,
                        Email = d.Email,
                        NIC = d.NIC,
                        PhoneNumber=d.ContactNo,
                        ProfileImage = d.ProfileImage,
                        CreatedAt = d.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                        UpdatedAt = d.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                        Status = d.Status,
                        LastLogin = d.LastLogin.HasValue ? d.LastLogin.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                    })
                  .FirstOrDefaultAsync();

                if (patient != null)
                {

                    var appointments = await (
                       from a in _context.Appointments
                        join d in _context.Doctor on a.DoctorId equals d.DoctorId
                        join h in _context.Hospitals on a.HospitalId equals h.HospitalId
                        where a.PatientId == patientId
                        select new AdminAllAppointmentsDto
                        {
                          AppointmentId = a.AppointmentId,
                          DoctorId = a.DoctorId,
                           DoctorName = d.FirstName + " " + d.LastName,
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

                    // Combine doctor details and availability
                    var patientAndAppointments = new AdminPatientAppointmentsDto()
                    {
                        Patient = patient,
                        Appointments = appointments
                    };
                    return patientAndAppointments;

                }
                else
                {
                    return null;
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<string> UpdatePatient(AdminUpdatePatientDto updatePatient)
        {
            if (updatePatient == null)
            {
                throw new ArgumentNullException(nameof(updatePatient));
            }
            try
            {
                var patient = _context.Patient.FirstOrDefault(p => p.PatientId == updatePatient.PatientId);
                if (patient == null)
                {
                    throw new Exception("Patient not found");
                }
                patient.Status = updatePatient.Status;
                patient.UpdatedAt = DateTime.UtcNow;
                _context.Patient.Update(patient);
                int result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    return "Success";
                }
                else
                {
                    return "Error";
                }

            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
