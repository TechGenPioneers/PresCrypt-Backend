using PresCrypt_Backend.PresCrypt.API.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PresCrypt_Backend.PresCrypt.Application.Services.DoctorPatientServices
{
    public interface IDoctorNotificationService
    {
        Task CreateAndSendNotificationAsync(string doctorId, string message, string title, string type);
        Task<List<DoctorNotificationDto>> GetNotificationsForDoctorAsync(string doctorId);
        Task MarkNotificationAsReadAsync(string notificationId);
        Task<bool> DeleteNotificationAsync(string notificationId);
    }
}