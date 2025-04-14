using PresCrypt_Backend.PresCrypt.Core.Models;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Application.Services.DoctorPrescriptionServices;

namespace PresCrypt_Backend.PresCrypt.Application.Services.DoctorPrescriptionServices
{
    public interface IDoctorPrescriptionSubmitService
    {
        Task<string> SubmitPrescriptionAsync(DoctorPrescriptionDto dto);
        Task<bool> SendToOpenMRSAsync(DoctorPrescriptionDto dto);
    }

}

