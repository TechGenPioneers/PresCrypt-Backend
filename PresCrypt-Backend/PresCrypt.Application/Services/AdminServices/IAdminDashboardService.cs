using PresCrypt_Backend.PresCrypt.API.Dto;

namespace PresCrypt_Backend.PresCrypt.Application.Services.AdminServices
{
    public interface IAdminDashboardService
    {
        public Task<AdmindashboardDto> GetDashboardData();

        Task CreateAndSendNotification(AdminNotificationDto adminNotification);
        Task<List<AdminNotificationDto>> GetNotifications();
        Task<string> MarkNotificationAsRead(string notificationId);
        public  Task<string> MarkAllAsRead();
    }
}
