using PresCrypt_Backend.PresCrypt.API.Dto;

namespace PresCrypt_Backend.PresCrypt.Application.Services.EmailServices.PatientEmailServices
{
    public interface IPatientEmailService
    {

        void SendEmail(PatientAppointmentEmailDto requets); 
    }
}
