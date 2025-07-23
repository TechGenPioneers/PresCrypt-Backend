using System.Threading.Tasks;
using PresCrypt_Backend.PresCrypt.API.Dto;

namespace PresCrypt_Backend.PresCrypt.Application.Services.DoctorPatientVideoServices
{
    public interface IVideoCallService
    {
        Task<string> CreateRoomAsync(string roomName);
        Task<string> GetRoomAsync(string roomId);
        Task<string> GenerateAccessUrlAsync(string roomId, string userRole);
        Task<DoctorPatientVideoCallDto> GetDoctorNameAsync(string doctorId);
        Task<DoctorPatientVideoCallDto> GetPatientNameAsync(string patientId);
    }
}
