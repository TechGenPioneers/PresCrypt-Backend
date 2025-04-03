using PresCrypt_Backend.PresCrypt.Core.Models;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Application.Services.AppointmentServices;

namespace PresCrypt_Backend.PresCrypt.Application.Services.AppointmentServices
{
    public interface IAppointmentService
    {
        Task<IEnumerable<AppointmentDisplayDto>> GetAppointmentsForTodayAsync(string doctorId);
        Task<IEnumerable<AppointmentDisplayDto>> GetAppointmentsByDateAsync(string date, string doctorId);
        Task<IEnumerable<AvailabilityDisplayDto>> GetAvailabilityByDateAsync(string day, string doctorId);

    }

}

