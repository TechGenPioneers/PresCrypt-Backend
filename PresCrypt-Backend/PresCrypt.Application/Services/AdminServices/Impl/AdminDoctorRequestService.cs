using Microsoft.EntityFrameworkCore;
using PresCrypt_Backend.PresCrypt.API.Dto;

namespace PresCrypt_Backend.PresCrypt.Application.Services.AdminServices.Impl
{
    public class AdminDoctorRequestService : IAdminDoctorRequestService
    {
        private readonly ApplicationDbContext _context;

        public AdminDoctorRequestService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<AdminAllDoctorRequestDto>> GetAllDoctorRequest()
        {

            var pndingDoctors = await _context.DoctorRequest
                .Select(d => new AdminAllDoctorRequestDto
                {
                    RequestId = d.RequestId,
                    FirstName = d.FirstName,
                    LastName = d.LastName,
                    Gender = d.Gender,
                    Specialization = d.Specialization,
                    CreatedAt = d.CreatedAt.ToString("yyyy-MM-dd"),
                    CheckedAt = d.CheckedAt.ToString("yyyy-MM-dd"),
                    Status = d.RequestStatus
                })
                .ToListAsync();

            return pndingDoctors;
        }
      
    }
}
