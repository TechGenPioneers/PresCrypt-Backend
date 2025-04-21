using Microsoft.EntityFrameworkCore;
using PresCrypt_Backend.PresCrypt.API.Dto;

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
                    .Select(patient => new
                    {
                        Patient = patient,
                        LastAppointment = _context.Appointments
                            .Where(a => a.PatientId == patient.PatientId)
                            .OrderByDescending(a => a.Date)
                            .Select(a => new
                            {
                                a.DoctorId,
                                a.Date,
                                a.Status
                            })
                            .FirstOrDefault()
                    })
                    .Join(
                        _context.Doctor,
                        x => x.LastAppointment.DoctorId,
                        doctor => doctor.DoctorId,
                        (x, doctor) => new AdminAllPatientDto
                        {
                            PatientId = x.Patient.PatientId,
                            FirstName = x.Patient.FirstName,
                            LastName = x.Patient.LastName,
                            DOB = x.Patient.DOB.ToString("yyyy-MM-dd"),
                            Gender = x.Patient.Gender,
                            LastAppointmentDoctorName = doctor.FirstName + " " + doctor.LastName,
                            LastAppointmentDoctorID = doctor.DoctorId,
                            LastAppointmentDate = x.LastAppointment.Date.ToString("yyyy-MM-dd"),
                            Status = x.LastAppointment.Status,
                            LastLogin = x.Patient.LastLogin.HasValue ? x.Patient.LastLogin.Value.ToString("yyyy-MM-dd HH:mm:ss") : null
                        }
                    )
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
                        BloodGroup = d.BloodGroup,
                        NIC = d.NIC,
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
    }
}
