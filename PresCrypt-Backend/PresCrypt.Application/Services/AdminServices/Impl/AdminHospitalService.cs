using Microsoft.EntityFrameworkCore;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Application.Services.AdminServices.Util;
using PresCrypt_Backend.PresCrypt.Core.Models;

namespace PresCrypt_Backend.PresCrypt.Application.Services.AdminServices.Impl
{
    public class AdminHospitalService : IAdminHospital
    {
        private readonly ApplicationDbContext _context;
        private readonly AdminDoctorUtil _adminDoctorUtil;
        public AdminHospitalService(ApplicationDbContext context , AdminDoctorUtil adminDoctorUtil)
        {
            _context = context;
            _adminDoctorUtil = adminDoctorUtil;
        }

        public async Task<List<AdminHospitalDto>> GetAllHospital()
        {
            var hospitals = await _context.Hospitals
                .Select(h => new AdminHospitalDto
                {
                    HospitalId = h.HospitalId,
                    HospitalName = h.HospitalName,
                    Number = h.Number,
                    Charge = h.Charge,
                    Address = h.Address,
                    City = h.City
                })
                .ToListAsync();

            return hospitals;
        }


        public async Task<AdminHospitalDto> GetHospitalById(string hospitalId)
        {
            var hospital = await _context.Hospitals
                .Where(d => d.HospitalId == hospitalId)
                .Select(d => new AdminHospitalDto
                {
                    HospitalId = d.HospitalId,
                    HospitalName = d.HospitalName,
                    Number = d.Number,
                    Charge = d.Charge,
                    Address = d.Address,
                    City = d.City
                })
                .FirstOrDefaultAsync();

            return hospital;
        }

        public async Task<bool> UpdateHospital(AdminHospitalDto hospitalDto)
        {
            var hospital = await _context.Hospitals.FindAsync(hospitalDto.HospitalId);

            if (hospital == null)
            {
                return false; // Hospital not found
            }

            hospital.HospitalName = hospitalDto.HospitalName;
            hospital.Number = hospitalDto.Number;
            hospital.Charge = hospitalDto.Charge;
            hospital.Address = hospitalDto.Address;
            hospital.City = hospitalDto.City;

            _context.Hospitals.Update(hospital);
            await _context.SaveChangesAsync();

            return true;
        }


        public async Task<bool> DeleteHospital(string hospitalId)
        {
            var hospital = await _context.Hospitals.FindAsync(hospitalId);

            if (hospital == null)
            {
                return false; // Hospital not found
            }

            _context.Hospitals.Remove(hospital);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<AdminHospitalDto> AddHospital(AdminHospitalDto hospitalDto)
        {
            var hospital = new Hospital
            {
                HospitalId = await _adminDoctorUtil.GenerateHospitalId(), 
                HospitalName = hospitalDto.HospitalName,
                Number = hospitalDto.Number,
                Charge = hospitalDto.Charge,
                Address = hospitalDto.Address,
                City = hospitalDto.City,
                DoctorAvailability = new List<DoctorAvailability>() 
            };

            _context.Hospitals.Add(hospital);
            await _context.SaveChangesAsync();

            return new AdminHospitalDto
            {
                HospitalId = hospital.HospitalId,
                HospitalName = hospital.HospitalName,
                Number = hospital.Number,
                Charge = hospital.Charge,
                Address = hospital.Address,
                City = hospital.City
            };
        }

    }
}
