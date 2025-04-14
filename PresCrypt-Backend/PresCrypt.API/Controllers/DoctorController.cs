using Mapster;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Application.Services.DoctorServices;
using PresCrypt_Backend.PresCrypt.Core.Models;
using static Azure.Core.HttpHeader;

namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowReactApp")]
    public class DoctorController : ControllerBase
    {
         private readonly ApplicationDbContext _context;
        private readonly IDoctorService _doctorServices;
        public DoctorController(ApplicationDbContext context, IDoctorService doctorServices)
        {
            _context = context;
            _doctorServices = doctorServices;
        }

        [HttpGet("search")]
        public async Task<ActionResult<List<DoctorSearchDto>>> GetDoctors([FromQuery] string specialization, [FromQuery] string hospitalName)//ORM Mapping
        {
            var doctors = await _doctorServices.GetDoctorAsync(specialization, hospitalName);
            return Ok(doctors);
            //var doctors = await _context.Doctors  //<--Here I used LINQ instead of ORM-->
            //    .Join(
            //        _context.Hospitals, 
            //        doctor => doctor.DoctorId, 
            //        hospital => hospital.DoctorId, 
            //        (doctor, hospital) => new { doctor, hospital }
            //    )
            //    .Where(dh =>
            //        (string.IsNullOrEmpty(specialization) || dh.doctor.Specialization.Contains(specialization)) && // Filter by specialization
            //        (string.IsNullOrEmpty(hospitalName) || dh.hospital.HospitalName.Contains(hospitalName)) // Filter by hospital name
            //    )
            //    .GroupJoin(
            //        _context.Doctor_Availability,
            //        doctor => doctor.doctor.DoctorId,
            //        doctorAvailability => doctorAvailability.DoctorId,
            //        (doctor, availability) => new DoctorSearchDto
            //        {
            //            DoctorName = doctor.doctor.DoctorName,
            //            AvailableDates = availability.Select(a => a.AvailableDate.ToDateTime(TimeOnly.MinValue)).ToList(), // Convert DateOnly to DateTime
            //            AvailableTimes = availability.Select(a => a.AvailableTime.ToTimeSpan()).ToList() // Convert time format
            //        }
            //    )
            //    .ToListAsync(); // Execute the query

            //return Ok(doctors);
        }


        //[HttpGet("search")] // <--------here there is an issue with mapster automatic mapping------->
        //public async Task<ActionResult<List<DoctorSearchDto>>> GetDoctors()
        //{
        //    var doctors = await _context.Doctors
        //        .GroupJoin(
        //            _context.Doctor_Availability,
        //            doctor => doctor.DoctorId,
        //            doctorAvailability => doctorAvailability.DoctorId,
        //            (doctor, availability) => new
        //            {
        //                Doctor = doctor,
        //                AvailableDates = availability.Select(a => a.AvailableDate).ToList(), // Corrected: Use AvailableDate
        //                AvailableTimes = availability.Select(a => a.AvailableTime.ToTimeSpan()).ToList() // Corrected: Use ToTimeSpan()
        //            })
        //        .ToListAsync();

        //    // Now, map the data from the anonymous object to DoctorDto using Mapster
        //    var response = doctors.Adapt<List<DoctorSearchDto>>();

        //    return Ok(response);
        //}




        [HttpGet("book/{doctorId}")]//for this I used mapster
        public async Task<ActionResult<List<DoctorBookingDto>>> GetDoctorBookedbyId(string doctorId)
        {
            var doctor = (await _context.Doctor.FindAsync(doctorId));
            if (doctor is null)
            {
                return NotFound();
            }
            var response = doctor.Adapt<DoctorBookingDto>();

            return Ok(response);

        }
        [HttpPost]
        [Route("DoctorRegistration")]
        public IActionResult DoctorRegistration(DoctorRegDTO doctorRegDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var objUser = _context.Doctor.FirstOrDefault(x => x.Email == doctorRegDTO.Email);
            if (objUser == null)
            {
                _ = _context.Doctor.Add(new Doctor
                {
                    DoctorId = Guid.NewGuid().ToString(),
                    DoctorName = doctorRegDTO.DoctorName,
                    Email = doctorRegDTO.Email,
                    Specialization = doctorRegDTO.Specialization,
                    SLMCRegId = doctorRegDTO.SLMCRegId,
                    EmailVerified = false,
                    PasswordHash = doctorRegDTO.Password,
                    NIC = doctorRegDTO.NIC
                });
                _context.SaveChanges();
                return Ok("Doctor Registered Successfully");
            }
            else
            {
                return BadRequest("Email Already Exists");
            }
        }

        [HttpPost]
        [Route("login")]
        public IActionResult Login(LoginDTO doctorLoginDTO)
        {
            Doctor? objUser = _context.Doctor.FirstOrDefault(x => x.Email == doctorLoginDTO.Email);
            if (objUser != null)
            {
                if (objUser.PasswordHash == doctorLoginDTO.Password)
                {
                    return Ok("Login Successful");
                }
                else
                {
                    return BadRequest("Invalid Password");
                }
            }
            else
            {
                return BadRequest("Invalid Email");
            }
        }
        [HttpGet]
        [Route("GetDoctor")]
        public IActionResult GetDoctor()
        {
            return Ok(_context.Doctor.ToList());
        }

        [HttpGet]
        [Route("GetDoctorByID")]
        public IActionResult GetDoctorById(string id)
        {
            var Doctor = _context.Doctor.FirstOrDefault(x => x.DoctorId == id);
            if (Doctor == null)
            {
                return NotFound("Doctor not found");
            }
            return Ok(Doctor);
        }

    }
}
