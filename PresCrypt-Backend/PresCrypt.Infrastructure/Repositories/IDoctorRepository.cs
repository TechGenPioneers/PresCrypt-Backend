using PresCrypt_Backend.PresCrypt.Core.Models;
using System.Threading.Tasks;

namespace PresCrypt_Backend.PresCrypt.Infrastructure.Repositories
{
    public interface IDoctorRepository
    {
        Task<Doctor> GetByIdAsync(string id);
    }
}