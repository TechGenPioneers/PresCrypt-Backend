namespace PresCrypt_Backend.PresCrypt.Application.Services.PatientServices
{
    public interface IPatientService
    {
        Task<IEnumerable<object>> GetAppointmentsForPatientAsync(string patientId);
        Task<(byte[] ImageData, string FileName)> GetProfileImageAsync(string patientId);
    }
}
