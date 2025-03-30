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

        public async Task<string> GenerateAvailabilityId()
        {
            // Get the last inserted Availability ID
            var lastAvailability = await _context.Doctor_Availability
                .OrderByDescending(d => d.AvailabilityId)
                .FirstOrDefaultAsync();

            if (lastAvailability == null)
                return "AV001"; // Start with AV001 if no availability exists

            // Extract the numeric part and increment it
            int lastNumber = int.Parse(lastAvailability.AvailabilityId.Substring(2)); // assuming AVXXX format

            // Format the new availability ID with leading zeros to ensure 3 digits
            return $"AV{(lastNumber + 1):D3}"; // Format as AV001, AV002, etc.
        }

    }
}
