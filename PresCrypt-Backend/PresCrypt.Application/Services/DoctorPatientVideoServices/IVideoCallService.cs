using System.Threading.Tasks;

namespace PresCrypt_Backend.PresCrypt.Application.Services.DoctorPatientVideoServices
{
    public interface IVideoCallService
    {
        Task<string> CreateRoomAsync(string roomName);
        Task<string> GetRoomAsync(string roomId);
        Task<string> GenerateAccessUrlAsync(string roomId, string userRole);
        Task<string> GetDoctorNameAsync(string doctorId);
        Task<string> GetPatientNameAsync(string patientId);
    }
}
