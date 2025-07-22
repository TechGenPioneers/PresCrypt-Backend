using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Core.Models;

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

        public async Task AddInquiryAsync(PatientContactUsDto dto)
        {
            
            var lastInquiry = await _context.PatientContactUs
                                    .OrderByDescending(i => i.InquiryId)
                                    .FirstOrDefaultAsync();

            string newId = "IN000";
            if (lastInquiry != null)
            {
                // Extract number and increment
                int lastNumber = int.Parse(lastInquiry.InquiryId.Substring(2));
                newId = "IN" + (lastNumber + 1).ToString("D3");
            }

            var entity = new PatientContactUs
            {
                InquiryId = newId,
                UserId = dto.UserId,     
                Role = dto.Role,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Topic = dto.Topic,
                Description = dto.Description
            };

            _context.PatientContactUs.Add(entity);
            await _context.SaveChangesAsync();
        }
        public async Task<PatientIdStatusDto?> GetPatientIdAndStatusByEmailAsync(string email)
        {
            var patient = await _context.Patient
                .Where(p => p.Email == email)
                .Select(p => new PatientIdStatusDto
                {
                    PatientId = p.PatientId,
                    Status = p.Status
                })
                .FirstOrDefaultAsync();

            return patient;
        }

        public async Task UpdateCancelStatusAsync(string patientId)
        {
            var patient = await _context.Patient.FirstOrDefaultAsync(p => p.PatientId == patientId);
            if (patient == null)
                throw new ArgumentException("Patient not found");

            var now = DateTime.UtcNow;

            if (patient.LastCancelledDate.HasValue)
            {
                var last = patient.LastCancelledDate.Value;
                var daysSinceLastCancel = (now - last).TotalDays;

                if (daysSinceLastCancel <= 14)
                {
                    patient.Status = "Inactive";
                }
            }

            // Always update LastCancelledDate to current time
            patient.LastCancelledDate = now;
            patient.UpdatedAt = now;

            await _context.SaveChangesAsync();
        }





    }
}
