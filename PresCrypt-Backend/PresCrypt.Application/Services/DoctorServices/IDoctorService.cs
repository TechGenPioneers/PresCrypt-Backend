namespace PresCrypt_Backend.PresCrypt.Application.Services.DoctorServices
{
    public interface IDoctorService
    {
        Task<List<DoctorSearchDto>> GetDoctorAsync(string specialization, string hospitalName, string name);
        Task<List<string>> GetAllSpecializationsAsync();

        Task<List<string>> GetAllDoctor();

        Task<IEnumerable<object>> GetDoctorAvailabilityByNameAsync(string doctorName);
    }
}
