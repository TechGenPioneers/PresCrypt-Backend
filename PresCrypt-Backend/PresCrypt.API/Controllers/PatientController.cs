using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PresCrypt_Backend.PresCrypt.Core.Models;
using PresCrypt_Backend.PresCrypt.API.Dto;

namespace PresCrypt_Backend.PresCrypt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientController : ControllerBase
    {
        private readonly ApplicationDbContext applicationDbContext;
        public PatientController(ApplicationDbContext applicationDbContext)
        {
            this.applicationDbContext = applicationDbContext;

        }
        [HttpPost]
        [Route("Registration")]
        public IActionResult Registration(PatientRegDTO patientRegDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var objUser = applicationDbContext.Patient.FirstOrDefault(x => x.Email == patientRegDTO.Email);
            if (objUser == null)
            {
                _ = applicationDbContext.Patient.Add(new Patient
                {
                    PatientId = Guid.NewGuid().ToString(), 
                    PatientName = patientRegDTO.FullName,
                    Email = patientRegDTO.Email,
                    PasswordHash = patientRegDTO.Password,
                    ContactNo = patientRegDTO.ContactNumber,
                    NIC = patientRegDTO.NIC,
                    BloodGroup = patientRegDTO.BloodGroup,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Status = patientRegDTO.Status
                });
                applicationDbContext.SaveChanges();
                return Ok("Patient Registered Successfully");
            }
            else
            {
                return BadRequest("Email Already Exists");
            }
        }

        [HttpPost]
        [Route("login")]
        public IActionResult Login(LoginDTO patientLoginDTO)
        {
            Patient? objUser = applicationDbContext.Patient.FirstOrDefault(x => x.Email == patientLoginDTO.Email);
            if (objUser != null)
            {
                if (objUser.PasswordHash == patientLoginDTO.Password)
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
        [Route("GetPatientById")]
        public IActionResult GetPatientById(string id)
        {
            var patient = applicationDbContext.Patient.FirstOrDefault(x => x.PatientId == id);
            if (patient != null)
            {
                return Ok(patient);
            }
            else
            {
                return BadRequest("Patient Not Found");
            }

        }

        [HttpGet]
        [Route("GetPatients")]
        public IActionResult GetPatients()
        {
            return Ok(applicationDbContext.Patient.ToList());
        }


    }
}