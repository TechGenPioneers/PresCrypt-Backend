using PresCrypt_Backend.PresCrypt.Core.Models;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Application.Services.AppointmentServices;

namespace PresCrypt_Backend.PresCrypt.Application.Services.AppointmentServices
{
    public interface IAppointmentService
    {
        Task<IEnumerable<AvailabilityDisplayDto>> GetAvailabilityByDateAsync(string day, string doctorId);
        Task<IEnumerable<AppointmentDisplayDto>> GetAppointmentsAsync(string doctorId, DateOnly? date = null);
        Task<Appointment> CreateAppointmentAsync(AppointmentSave dto);

        Task<IEnumerable<AppointmentDisplayDto>> GetRecentAppointmentsByDoctorAsync(string doctorId);   //for prescription page to get recent appointments

        Task<List<PatientAppointmentListDto>> GetAppointmentsByPatientIdAsync(string patientId);
        Task<Dictionary<DateTime, int>> GetAppointmentCountsAsync(string doctorId, List<DateTime> dates);

        //for reschedule
        Task<List<AppointmentRescheduleDto>> GetAvailableHospitalsByDateAsync(DateTime date, string doctorId);
        Task<int> RescheduleAppointmentsAsync(AppointmentRescheduleDto dto);


    }

}

