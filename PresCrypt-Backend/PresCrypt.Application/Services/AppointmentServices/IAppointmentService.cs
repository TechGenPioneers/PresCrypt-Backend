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

        //for prescription page to get recent appointments
        Task<IEnumerable<AppointmentDisplayDto>> GetRecentAppointmentsByDoctorAsync(string doctorId);  

        Task<List<PatientAppointmentListDto>> GetAppointmentsByPatientIdAsync(string patientId);
        Task<bool> DeleteAppointmentAsync(string appointmentId);
        Task<Dictionary<DateTime, int>> GetAppointmentCountsAsync(string doctorId, List<DateTime> dates);

        //for reschedule
        Task<DateTime?> GetNextAvailableSlotAsync(string doctorId, string hospitalId, DateTime afterDate);
        Task<List<AppointmentRescheduleResultDto>> RescheduleAppointmentsAsync(List<string> appointmentIds);
        Task<List<AppointmentRescheduleDto>> GetAvailableHospitalsByDateAsync(DateTime date, string doctorId);
        //Task<int> RescheduleAppointmentsAsync(AppointmentRescheduleDto dto);
        Task<List<PatientAppointmentListDto>> GetAppointmentsByDateRangeAsync(DateOnly startDate, DateOnly endDate, string? PatientId);
        Task CancelAppointmentAsync(string appointmentId, string patientId);

        Task<IEnumerable<AppointmentViewDialogDto>> GetAppointmentsByPatientIdAndDateAsync(string patientId, DateTime date);
    }

}


