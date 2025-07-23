
﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PresCrypt_Backend.PresCrypt.Application.Services.OpenMrsServices;

namespace PresCrypt_Backend.PresCrypt.API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class PatientObservationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IOpenMrsObsService _obsService;

        public PatientObservationsController(ApplicationDbContext context, IOpenMrsObsService obsService)
        {
            _context = context;
            _obsService = obsService;
        }

        [HttpGet("{patientId}")]
        public async Task<IActionResult> GetObservations(string patientId)
        {
            var patient = await _context.Patient.FirstOrDefaultAsync(p => p.PatientId == patientId);

            if (patient == null)
                return NotFound("Patient not found");

            if (string.IsNullOrWhiteSpace(patient.OpenMrsId))
                return BadRequest("Patient does not have a linked OpenMRS ID");

            var observationsJson = await _obsService.GetObservationsByPatientIdAsync(patient.OpenMrsId);

            return Ok(new { data = observationsJson }); // Or deserialize if needed
        }
    }
}

