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

        public async Task<List<AdminAllDoctorRequestDto>> GetAllPendingDoctors()
        {

            var pndingDoctors = await _context.DoctorRequest
                .Where(d => d.RequestStatus == "Pending")
                .Select(d => new AdminAllDoctorRequestDto
                {
                    RequestId = d.RequestId,
                    FirstName = d.FirstName,
                    LastName = d.LastName,
                    Gender = d.Gender,
                    Specialization = d.Specialization,
                    CreatedAt = d.CreatedAt
                })
                .ToListAsync();

            return pndingDoctors;
        }
        public async Task<List<AdminAllDoctorRequestDto>> GetAllApprovedDoctors()
        {

            var approvedDoctors = await _context.DoctorRequest
                .Where(d => d.RequestStatus == "Approved")
                .Select(d => new AdminAllDoctorRequestDto
                {
                    RequestId = d.RequestId,
                    FirstName = d.FirstName,
                    LastName = d.LastName,
                    Gender = d.Gender,
                    Specialization = d.Specialization,
                    CreatedAt = d.CreatedAt
                })
                .ToListAsync();

            return approvedDoctors;
        }
        public async Task<List<AdminAllDoctorRequestDto>> GetAllRejectedDoctors()
        {

            var rejectedDoctors = await _context.DoctorRequest
                .Where(d => d.RequestStatus == "Rejected")
                .Select(d => new AdminAllDoctorRequestDto
                {
                    RequestId = d.RequestId,
                    FirstName = d.FirstName,
                    LastName = d.LastName,
                    Gender = d.Gender,
                    Specialization = d.Specialization,
                    CreatedAt = d.CreatedAt
                })
                .ToListAsync();

            return rejectedDoctors;
        }
    }
}
