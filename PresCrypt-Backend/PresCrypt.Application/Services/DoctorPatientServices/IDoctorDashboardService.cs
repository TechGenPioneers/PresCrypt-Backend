using PresCrypt_Backend.PresCrypt.Core.Models;
using PresCrypt_Backend.PresCrypt.API.Dto;

public interface IDoctorDashboardService
{
    Task<DoctorDashboardDto> GetDashboardStatsAsync(string doctorId);
    Task<DoctorProfileDto> GetDoctorProfileAsync(string doctorId);
}
