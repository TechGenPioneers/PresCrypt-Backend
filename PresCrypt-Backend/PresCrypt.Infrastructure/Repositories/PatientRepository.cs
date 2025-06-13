using PresCrypt_Backend.PresCrypt.Core.Models;
using System.Threading.Tasks;

namespace PresCrypt_Backend.PresCrypt.Infrastructure.Repositories
{
    public class PatientRepository : IPatientRepository
    {
        private readonly ApplicationDbContext _context;

        public PatientRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Patient> GetByIdAsync(string id)
        {
            return await _context.Patient.FindAsync(id);
        }
    }
}