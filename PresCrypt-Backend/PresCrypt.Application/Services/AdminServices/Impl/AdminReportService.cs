using Microsoft.EntityFrameworkCore;
using PresCrypt_Backend.PresCrypt.API.Dto;

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
    }
}
