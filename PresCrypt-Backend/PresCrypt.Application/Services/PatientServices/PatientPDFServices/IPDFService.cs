using PresCrypt_Backend.PresCrypt.API.Dto;

namespace PresCrypt_Backend.PresCrypt.Application.Services.PatientServices.PatientPDFServices
{
    public interface IPDFService
    {
        byte[] GeneratePDF(AppointmentPDFDetailsDto details);
        Task<byte[]> GeneratePdfAsync(List<PatientAppointmentListDto> appointments);

    }
}
