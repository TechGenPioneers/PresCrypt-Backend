using System.Collections.Generic;
using System.Threading.Tasks;

namespace PresCrypt_Backend.PresCrypt.Application.Services.HospitalServices
{
    public interface IHospitalService
    {
        Task<Dictionary<string, List<string>>> GetHospitalsGroupedByCityAsync();
    }
}
