using Microsoft.EntityFrameworkCore;

namespace PresCrypt_Backend.PresCrypt.Application.Services.AdminServices.Util
{
    public class AdminDoctorUtil
    {
        private readonly ApplicationDbContext _context;
            public AdminDoctorUtil(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateDoctorId()
        {
            // Get the last inserted doctor ID
            var lastDoctor = await _context.Doctors
                .OrderByDescending(d => d.DoctorId)
                .FirstOrDefaultAsync();

            if (lastDoctor == null)
                return "D001"; // Start with D001 if no doctors exist

            // Extract the numeric part and increment it
            int lastNumber = int.Parse(lastDoctor.DoctorId.Substring(1));

            // Format the new doctor ID with leading zeros to ensure 3 digits
            return $"D{(lastNumber + 1):D3}"; // Format as D001, D002, etc.
        }

    }
}
