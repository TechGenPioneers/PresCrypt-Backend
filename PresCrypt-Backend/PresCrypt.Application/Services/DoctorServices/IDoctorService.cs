namespace PresCrypt_Backend.PresCrypt.Application.Services.DoctorServices
{
    public interface IDoctorService
    {
        Task<List<DoctorSearchDto>> GetDoctorAsync(string specialization, string hospitalName);//Task is the return type and returns a List of doctors 
    }
}
