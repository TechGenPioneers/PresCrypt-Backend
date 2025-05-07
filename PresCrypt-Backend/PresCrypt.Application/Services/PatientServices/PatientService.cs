using Microsoft.EntityFrameworkCore;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PresCrypt_Backend.PresCrypt.Application.Services.PatientServices
{
    public class PatientService : IPatientService
    {
        private readonly ApplicationDbContext _context;

        public PatientService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<object>> GetAppointmentsForPatientAsync(string patientId)
        {
            return await _context.Appointments
                .Where(a => a.PatientId == patientId)
                .Select(a => new
                {
                    a.Date,
                    a.Status
                })
                .ToListAsync();
        }

        public async Task<(byte[] ImageData, string FileName)> GetProfileImageAsync(string patientId)
        {
            var patient = await _context.Patient.FindAsync(patientId);
            if (patient == null || patient.ProfileImage == null || patient.ProfileImage.Length == 0)
                return (null, null);

            return (patient.ProfileImage, patient.FirstName);
        }

        public async Task<PatientNavBarDto> GetPatientNavBarDetailsAsync(string patientId)
        {
            var patient = await _context.Patient
                .Where(p => p.PatientId == patientId)
                .Select(p => new PatientNavBarDto
                {
                    Name = p.FirstName + " " + p.LastName,
                    DOB = p.DOB,
                    CreatedAt = p.CreatedAt
                })
                .FirstOrDefaultAsync();

            return patient;
        }
    }
}
