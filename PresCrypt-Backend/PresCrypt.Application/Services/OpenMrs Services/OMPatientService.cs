using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;



namespace PresCrypt_Backend.PresCrypt.Application.Services.OpenMrs_Services
{
    public interface OPatientService
    {
        Task<JsonDocument> GetPatientObservations(Guid patientId);
    }

    public class OMPatientService : OPatientService
    {
        private readonly IOpenMrsService _openMrsService;
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<OMPatientService> _logger;

        public OMPatientService(
            IOpenMrsService openMrsService,
            ApplicationDbContext dbContext,
            ILogger<OMPatientService> logger)
        {
            _openMrsService = openMrsService;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<JsonDocument> GetPatientObservations(Guid patientId)
        {
            // 1. Get patient OpenMrsId from database
            var patient = await _dbContext.PatientProfiles
                .Where(p => p.Id == patientId)
                .FirstOrDefaultAsync();

            if (patient == null)
            {
                _logger.LogWarning($"Patient with ID {patientId} not found");
                throw new KeyNotFoundException($"Patient with ID {patientId} not found");
            }

            if (string.IsNullOrEmpty(patient.OpenMrsId))
            {
                _logger.LogWarning($"Patient with ID {patientId} has no OpenMRS ID");
                throw new InvalidOperationException($"Patient with ID {patientId} has no OpenMRS ID assigned");
            }

            // 2. Use the OpenMRS service to get observations data
            return await _openMrsService.GetOpenMrsData("obs", patient.OpenMrsId);
        }
    }
}
