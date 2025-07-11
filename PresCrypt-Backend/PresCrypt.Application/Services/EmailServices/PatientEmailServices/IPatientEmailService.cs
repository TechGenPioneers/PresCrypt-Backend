using PresCrypt_Backend.PresCrypt.API.Dto;
using System;
using System.Threading.Tasks;

namespace PresCrypt_Backend.PresCrypt.Application.Services.EmailServices.PatientEmailServices
{
    public interface IPatientEmailService
    {

        void SendEmail(PatientAppointmentEmailDto requets);
        Task SendRescheduleConfirmationEmailAsync(AppointmentRescheduleEmailDto request);
    }
}
