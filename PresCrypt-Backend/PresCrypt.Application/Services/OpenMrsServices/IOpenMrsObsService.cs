namespace PresCrypt_Backend.PresCrypt.Application.Services.OpenMrsServices
{
    public interface IOpenMrsObsService
    {
        Task<string> GetObservationsByPatientIdAsync(string openMrsId);
    }
}
