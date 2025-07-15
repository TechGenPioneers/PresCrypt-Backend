using PresCrypt_Backend.PresCrypt.API.Dto;
using static PresCrypt_Backend.PresCrypt.API.Dto.PatientContactUsDto;

namespace PresCrypt_Backend.PresCrypt.Application.Services.PatientServices
{
    public interface IPatientService
    {
        Task<IEnumerable<object>> GetAppointmentsForPatientAsync(string patientId);
        Task<(byte[] ImageData, string FileName)> GetProfileImageAsync(string patientId);

        Task<PatientNavBarDto> GetPatientNavBarDetailsAsync(string patientId);

        Task AddInquiryAsync(PatientContactUsDto dto);

        Task<string?> GetPatientIdByEmailAsync(string email);

    }
}
