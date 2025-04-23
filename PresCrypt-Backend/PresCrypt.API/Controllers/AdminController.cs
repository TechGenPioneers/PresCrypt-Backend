
//using Microsoft.AspNetCore.Mvc;
//using PresCrypt_Backend.PresCrypt.Core.Models;
//using PresCrypt_Backend.PresCrypt.API.Dto;
//using System.Reflection.Metadata.Ecma335;

//namespace PresCrypt_Backend.PresCrypt.API.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class AdminController : ControllerBase
//    {
//        private readonly ApplicationDbContext applicationDbContext;
//        public AdminController(ApplicationDbContext applicationDbContext)
//        {
//            this.applicationDbContext = applicationDbContext;
//        }
//        [HttpPost]
//        [Route("AdminRegistration")]
//        public IActionResult AdminRegistration(AdminRegDTO adminRegDTO)
//        {
//            if (!ModelState.IsValid)
//            {
//                return BadRequest(ModelState);
//            }
//            var objUser = applicationDbContext.Admin.FirstOrDefault(x => x.Email == adminRegDTO.Email);
//            if (objUser == null)
//            {
//                _ = applicationDbContext.Admin.Add(new Admin
//                {
//                    AdminId = Guid.NewGuid().ToString(),
//                    AdminName = adminRegDTO.FullName,
//                    Email = adminRegDTO.Email,
//                    PasswordHash = adminRegDTO.Password,
//                    CreatedAt = DateTime.UtcNow,
//                    UpdatedAt = DateTime.UtcNow,
//                    Status = adminRegDTO.Status,
//                    Role = adminRegDTO.Role
//                });
//                applicationDbContext.SaveChanges();
//                return Ok("Admin Registered Successfully");
//            }
//            else
//            {
//                return BadRequest("Email already exists");
//            }
//        }
//        [HttpPost]
//        [Route("login")]
//        public IActionResult Login(LoginDTO adminLoginDTO)
//        {
//            Admin? objUser = applicationDbContext.Admin.FirstOrDefault(x => x.Email == adminLoginDTO.Email);
//            if (objUser != null)
//            {
//                if (objUser.PasswordHash == adminLoginDTO.Password)
//                {
//                    return Ok("Login Successful");
//                }
//                else
//                {
//                    return BadRequest("Invalid Password");
//                }
//            }
//            else
//            {
//                return BadRequest("Invalid Email");
//            }
//        }

//        [HttpGet]
//        [Route("GetAdminById")]
//        public IActionResult GetAdminByIf(string id)
//        {
//            var admin = applicationDbContext.Admin.FirstOrDefault(x => x.AdminId == id);
//            if (admin != null)
//            {
//                return Ok(admin);
//            }
//            else
//            {
//                return BadRequest("Admin Not Found");
//            }

//        }

//        [HttpGet]
//        [Route("GetAdmins")]
//        public IActionResult GetAdmins()
//        {
//            return Ok(applicationDbContext.Admin.ToList());
//        }


//    }
//}



// endponts related ti adminn athentication

