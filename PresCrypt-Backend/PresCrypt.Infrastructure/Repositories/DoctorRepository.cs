using PresCrypt_Backend.PresCrypt.Core.Models;
using System.Threading.Tasks;

namespace PresCrypt_Backend.PresCrypt.Infrastructure.Repositories
{
    public class DoctorRepository : IDoctorRepository
    {
        private readonly ApplicationDbContext _context;

        public DoctorRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Doctor> GetByIdAsync(string id)
        {
            return await _context.Doctor.FindAsync(id);
        }
    }
}