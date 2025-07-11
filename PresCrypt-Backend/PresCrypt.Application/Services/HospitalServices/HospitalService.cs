using Microsoft.EntityFrameworkCore;
using PresCrypt_Backend.PresCrypt.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PresCrypt_Backend.PresCrypt.Application.Services.HospitalServices
{
    public class HospitalService : IHospitalService
    {
        private readonly ApplicationDbContext _context;

        public HospitalService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Dictionary<string, List<string>>> GetHospitalsGroupedByCityAsync()
        {
            return await _context.Hospitals
                .GroupBy(h => h.City)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.Select(h => h.HospitalName).Distinct().ToList()
                );
        }
    }
}
