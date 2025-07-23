using PresCrypt_Backend.PresCrypt.API.Dto;
using System.Text.Json;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace PresCrypt_Backend.PresCrypt.Application.Services.OpenMrsServices
{
    public class OpenMrsService : IOpenMrsService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _username;
        private readonly string _password;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OpenMrsService> _logger;

        // OpenMRS Concept UUIDs
        private readonly Dictionary<string, string> _conceptUuids = new()
        {
            { "Height", "5090AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" },
            { "Weight", "5089AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" },
            { "BloodType", "beafa09f-d7cf-4439-8384-57cf22a3ac09" },
            { "BloodSugar", "887AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" },
            { "HeartRate", "5087AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" },
            { "SystolicBP", "5085AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" },
            { "DiastolicBP", "5086AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" },
            { "Allergies", "fb23caa9-8f4e-4821-9335-2372f34139de" }
        };

        public OpenMrsService(
            HttpClient httpClient,
            IConfiguration configuration,
            ApplicationDbContext context,
            ILogger<OpenMrsService> logger)
        {
            _httpClient = httpClient;
            _context = context;
            _logger = logger;

            _baseUrl = configuration.GetValue<string>("OpenMRS:BaseUrl") ?? "http://localhost:80/openmrs";
            _username = configuration.GetValue<string>("OpenMRS:Username") ?? "admin";
            _password = configuration.GetValue<string>("OpenMRS:Password") ?? "Admin123";

            // Setup Basic Authentication
            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_username}:{_password}"));
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<string> GetPatientOpenMrsIdAsync(string patientId)
        {
            try
            {
                _logger.LogInformation("Looking up OpenMRS ID for patient: {PatientId}", patientId);

                var patient = await _context.Patient
                    .FirstOrDefaultAsync(p => p.PatientId == patientId);

                if (patient == null)
                {
                    _logger.LogWarning("Patient with ID {PatientId} not found in database", patientId);
                    throw new ArgumentException($"Patient with ID {patientId} not found");
                }

                if (string.IsNullOrEmpty(patient.OpenMrsId))
                {
                    _logger.LogWarning("Patient {PatientId} exists but has no OpenMRS ID", patientId);
                    throw new ArgumentException($"Patient with ID {patientId} has no OpenMRS ID");
                }

                _logger.LogInformation("Found OpenMRS ID {OpenMrsId} for patient {PatientId}",
                    patient.OpenMrsId, patientId);

                return patient.OpenMrsId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving OpenMRS ID for patient {PatientId}", patientId);
                throw;
            }
        }

        public async Task<bool> CreateObservationsAsync(PatientVitalsDto vitals)
        {
            try
            {
                var openMrsPersonId = await GetPatientOpenMrsIdAsync(vitals.PatientId);
                var obsDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");
                var observations = new List<OpenMrsObsDto>();

                // Create observations for each vital sign
                if (vitals.Height.HasValue)
                {
                    observations.Add(new OpenMrsObsDto
                    {
                        Person = openMrsPersonId,
                        Concept = _conceptUuids["Height"],
                        ObsDatetime = obsDateTime,
                        Value = vitals.Height.Value
                    });
                }

                if (vitals.Weight.HasValue)
                {
                    observations.Add(new OpenMrsObsDto
                    {
                        Person = openMrsPersonId,
                        Concept = _conceptUuids["Weight"],
                        ObsDatetime = obsDateTime,
                        Value = vitals.Weight.Value
                    });
                }

                if (!string.IsNullOrEmpty(vitals.BloodGroup))
                {
                    observations.Add(new OpenMrsObsDto
                    {
                        Person = openMrsPersonId,
                        Concept = _conceptUuids["BloodType"],
                        ObsDatetime = obsDateTime,
                        Value = vitals.BloodGroup
                    });
                }

                if (vitals.BloodSugar.HasValue)
                {
                    observations.Add(new OpenMrsObsDto
                    {
                        Person = openMrsPersonId,
                        Concept = _conceptUuids["BloodSugar"],
                        ObsDatetime = obsDateTime,
                        Value = vitals.BloodSugar.Value
                    });
                }

                if (vitals.HeartRate.HasValue)
                {
                    observations.Add(new OpenMrsObsDto
                    {
                        Person = openMrsPersonId,
                        Concept = _conceptUuids["HeartRate"],
                        ObsDatetime = obsDateTime,
                        Value = vitals.HeartRate.Value
                    });
                }

                if (vitals.SystolicBloodPressure.HasValue)
                {
                    observations.Add(new OpenMrsObsDto
                    {
                        Person = openMrsPersonId,
                        Concept = _conceptUuids["SystolicBP"],
                        ObsDatetime = obsDateTime,
                        Value = vitals.SystolicBloodPressure.Value
                    });
                }

                if (vitals.DiastolicBloodPressure.HasValue)
                {
                    observations.Add(new OpenMrsObsDto
                    {
                        Person = openMrsPersonId,
                        Concept = _conceptUuids["DiastolicBP"],
                        ObsDatetime = obsDateTime,
                        Value = vitals.DiastolicBloodPressure.Value
                    });
                }

                if (!string.IsNullOrEmpty(vitals.Allergies))
                {
                    observations.Add(new OpenMrsObsDto
                    {
                        Person = openMrsPersonId,
                        Concept = _conceptUuids["Allergies"],
                        ObsDatetime = obsDateTime,
                        Value = vitals.Allergies
                    });
                }

                // Send all observations to OpenMRS
                var results = await Task.WhenAll(observations.Select(SendObservationAsync));

                bool allSuccessful = results.All(result => result);

                if (allSuccessful)
                {
                    _logger.LogInformation("Successfully created {Count} observations for patient {PatientId}",
                        observations.Count, vitals.PatientId);
                }
                else
                {
                    _logger.LogWarning("Some observations failed to create for patient {PatientId}", vitals.PatientId);
                }

                return allSuccessful;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating observations for patient {PatientId}", vitals.PatientId);
                return false;
            }
        }

        private async Task<bool> SendObservationAsync(OpenMrsObsDto observation)
        {
            try
            {
                var json = JsonSerializer.Serialize(observation, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                _logger.LogInformation("Sending observation to OpenMRS: {Json}", json);

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/ws/rest/v1/obs", content);

                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully created observation for concept {Concept}. Response: {Response}",
                        observation.Concept, responseContent);
                    return true;
                }
                else
                {
                    _logger.LogError("Failed to create observation. Status: {Status}, Error: {Error}, Request: {Request}",
                        response.StatusCode, responseContent, json);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while sending observation for concept {Concept}",
                    observation.Concept);
                return false;
            }
        }
    }
}
