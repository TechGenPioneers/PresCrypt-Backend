using Microsoft.EntityFrameworkCore;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Application.Services.EmailServices;
using PresCrypt_Backend.PresCrypt.Application.Services.EmailServices.Impl;
using System.Diagnostics;

namespace PresCrypt_Backend.PresCrypt.Application.Services.AdminServices.Impl
{
    public class AdminDoctorRequestService : IAdminDoctorRequestService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAdminDashboardService _adminDashboardService;
        private readonly IAdminEmailService _adminEmailService;

        public AdminDoctorRequestService(ApplicationDbContext context, IAdminDashboardService adminDashboardService, IAdminEmailService adminEmailService)
        {
            _context = context;
            _adminDashboardService=adminDashboardService;
            _adminEmailService=adminEmailService;
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
                    CheckedAt = d.CheckedAt.HasValue ? d.CheckedAt.Value.ToString("yyyy-MM-dd") : null,
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
                    SLMCIdImage=d.SLMCIdImage,
                    NIC = d.NIC,
                    Charge=d.Charge,
                    EmailVerified = d.EmailVerified,
                    RequestStatus = d.RequestStatus,
                    CreatedAt = d.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    CheckedAt = d.CheckedAt.HasValue ? d.CheckedAt.Value.ToString("yyyy-MM-dd") : null,
                    Reason = d.Reason,
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

        public async Task<string> RejectRequest(DoctorRequestRejectDto rejected)
        {
            var doctorRequest = await _context.DoctorRequest.FirstOrDefaultAsync(d => d.RequestId == rejected.RequestId);
            if (doctorRequest == null)
            {
                return "Doctor request not found.";
            }

            try
            {
                // Fetch the request
                var request = await _context.DoctorRequest
                    .FirstOrDefaultAsync(d => d.RequestId == rejected.RequestId);

                doctorRequest.RequestStatus = "Rejected";
                doctorRequest.CheckedAt = DateTime.Now;
                doctorRequest.Reason = rejected.Reason;
                _context.DoctorRequest.Update(doctorRequest);
                int result = await _context.SaveChangesAsync();

                // Prepare notification details
                var message = $"{rejected.RequestId} {request.FirstName} {request.LastName} - Request Rejected.";
                var notificationDto = new AdminNotificationDto
                {
                    RequestId = rejected.RequestId,
                    Message = message,
                    Title = "Doctor Request Rejected",
                    Type = "Alert"
                };

                try
                {
                    //call the notification service
                    await _adminDashboardService.CreateAndSendNotification(notificationDto);
                }
                catch (Exception ex)
                {
                    return $"Unexpected error: {ex.Message} ";
                }

                return result > 0 ? "Success" : "Error";
            }
            catch (Exception e)
            {
                return $"Unexpected error: {e.Message} \nStackTrace: {e.StackTrace}";
            }
        }

        public async Task<string> ApprovRequest(string requestId)
        {
            var doctorRequest = await _context.DoctorRequest.FirstOrDefaultAsync(d => d.RequestId == requestId);
            if (doctorRequest == null)
            {
                return "Doctor request not found.";
            }

            AdminDoctorRequestDto doctor = new AdminDoctorRequestDto
            {
                FirstName = doctorRequest.FirstName,
                LastName = doctorRequest.LastName,
                Email = doctorRequest.Email
            };


            try
            {
                // Fetch the request
                var request = await _context.DoctorRequest
                    .FirstOrDefaultAsync(d => d.RequestId == requestId);

                doctorRequest.RequestStatus = "Approved";
                doctorRequest.CheckedAt = DateTime.Now;
                _context.DoctorRequest.Update(doctorRequest);
                int result = await _context.SaveChangesAsync();

                // Prepare notification details
                var message = $"{requestId} {request.FirstName} {request.LastName} - Request Approved.";
                var notificationDto = new AdminNotificationDto
                {
                    RequestId = requestId,
                    Message = message,
                    Title = "Doctor Request Approved",
                    Type = "Alert"
                };

                try
                {
                    //call the notification service
                    await _adminDashboardService.CreateAndSendNotification(notificationDto);
                }
                catch (Exception ex)
                {
                    return $"Unexpected error: {ex.Message} ";
                }

                try
                {

                    await _adminEmailService.ApproveEmail(doctor);
                }
                catch (Exception ex)
                {
                    return $"Unexpected error: {ex.Message} ";
                }

                return result > 0 ? "Success" : "Error";
            }
            catch (Exception e)
            {
                return $"Unexpected error: {e.Message} \nStackTrace: {e.StackTrace}";
            }
        }
    }
}
