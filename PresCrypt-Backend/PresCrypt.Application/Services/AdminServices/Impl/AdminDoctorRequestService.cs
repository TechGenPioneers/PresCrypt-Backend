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


        public async Task<RequestDoctorAvailabilityDto> getRequestByID(string requestID)
        {
            // Fetch request details
            var getRequest = await _context.DoctorRequest
                .Where(d => d.RequestId == requestID)
                .Select(d => new AdminDoctorRequestDto
                {
                    RequestId = d.RequestId,
                    FirstName = d.FirstName,
                    LastName = d.LastName,
                    Gender = d.Gender,
                    Email = d.Email,
                    Specialization = d.Specialization,
                    SLMCRegId = d.SLMCRegId,
                    NIC = d.NIC,
                    Charge=d.Charge,
                    EmailVerified = d.EmailVerified,
                    RequestStatus = d.RequestStatus,
                    CreatedAt = d.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    CheckedAt = d.CheckedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    ContactNumber = d.ContactNumber
                })
                .FirstOrDefaultAsync();

            // If request not found, return null
            if (getRequest != null)
            {

                // Fetch doctor availability along with hospital names
                var getDoctorAvailability = await _context.RequestAvailability
                    .Where(d => d.DoctorRequestId == requestID)
                    .Join(
                        _context.Hospitals,
                        a => a.HospitalId,
                        h => h.HospitalId,
                        (a, h) => new DoctorRequestAvailabilityDto
                        {
                            AvailabilityRequestId = a.AvailabilityRequestId.ToString(),
                            DoctorRequestId=a.DoctorRequestId,
                            AvailableDay = a.AvailableDay,
                            AvailableStartTime = a.AvailableStartTime.ToString("HH:mm"),
                            AvailableEndTime = a.AvailableEndTime.ToString("HH:mm"),
                            HospitalName = h.HospitalName,
                            HospitalId = h.HospitalId
                        }
                    )
                    .ToListAsync();

                // Combine doctor details and availability
                var requestAndAvailability = new RequestDoctorAvailabilityDto()
                {
                    Request = getRequest,
                    RequestAvailability = getDoctorAvailability
                };
                return requestAndAvailability;
            }
            else
            {
                return null;
            }

        }
    }
}
