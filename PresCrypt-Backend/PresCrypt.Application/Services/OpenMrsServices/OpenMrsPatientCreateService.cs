using PresCrypt_Backend.PresCrypt.API.Dto;
using PresCrypt_Backend.PresCrypt.Core.Models;
using System.Text.Json;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;

namespace PresCrypt_Backend.PresCrypt.Application.Services.OpenMrsServices
{
    public interface IOpenMrsPatientCreateService
    {
        Task<OpenMrsPatientResponseDto> CreatePatientInOpenMrsAsync(string patientId);
    }

    public class OpenMrsPatientCreateService : IOpenMrsPatientCreateService
    {
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OpenMrsPatientCreateService> _logger;

        // OpenMRS configuration constants
        private const string OPENMRS_BASE_URL = "http://localhost:80/openmrs/ws/rest/v1";
        private const string IDENTIFIER_TYPE_UUID = "05a29f94-c0ed-11e2-94be-8c13b969e334";
        private const string LOCATION_UUID = "aff27d58-a15c-49a6-9beb-d30dcfc0c66e";
        private const string OPENMRS_USERNAME = "admin";
        private const string OPENMRS_PASSWORD = "Admin123";

        public OpenMrsPatientCreateService(
            HttpClient httpClient,
            ApplicationDbContext context,
            ILogger<OpenMrsPatientCreateService> logger)
        {
            _httpClient = httpClient;
            _context = context;
            _logger = logger;

            // Configure Basic Authentication for OpenMRS
            ConfigureAuthentication();
        }

        private void ConfigureAuthentication()
        {
            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{OPENMRS_USERNAME}:{OPENMRS_PASSWORD}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<OpenMrsPatientResponseDto> CreatePatientInOpenMrsAsync(string patientId)
        {
            try
            {
                // Step 1: Get patient data from local database
                var patient = await _context.Patient
                    .FirstOrDefaultAsync(p => p.PatientId == patientId);

                if (patient == null)
                {
                    _logger.LogWarning("Patient with ID {PatientId} not found in local database", patientId);
                    return new OpenMrsPatientResponseDto
                    {
                        Success = false,
                        Message = $"Patient with ID {patientId} not found in local database"
                    };
                }

                // Check if patient already has OpenMRS ID
                if (!string.IsNullOrEmpty(patient.OpenMrsId))
                {
                    _logger.LogInformation("Patient {PatientId} already has OpenMRS ID: {OpenMrsId}", patientId, patient.OpenMrsId);
                    return new OpenMrsPatientResponseDto
                    {
                        OpenMrsId = patient.OpenMrsId,
                        Success = true,
                        Message = "Patient already exists in OpenMRS"
                    };
                }

                // Step 2: Get next identifier from OpenMRS
                var identifier = await GetNextIdentifierAsync();
                if (string.IsNullOrEmpty(identifier))
                {
                    return new OpenMrsPatientResponseDto
                    {
                        Success = false,
                        Message = "Failed to get identifier from OpenMRS"
                    };
                }

                _logger.LogInformation("Generated identifier {Identifier} for patient {PatientId}", identifier, patientId);

                // Step 3: Create patient in OpenMRS
                var openMrsPatient = MapToOpenMrsPatient(patient, identifier);
                var createdPatient = await CreatePatientAsync(openMrsPatient);

                if (createdPatient != null)
                {
                    // Step 4: Update local patient record with OpenMRS ID
                    patient.OpenMrsId = createdPatient.Uuid;
                    patient.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Successfully created patient in OpenMRS. PatientId: {PatientId}, OpenMrsId: {OpenMrsId}",
                        patientId, createdPatient.Uuid);

                    return new OpenMrsPatientResponseDto
                    {
                        OpenMrsId = createdPatient.Uuid,
                        Identifier = identifier,
                        Success = true,
                        Message = "Patient successfully created in OpenMRS"
                    };
                }

                return new OpenMrsPatientResponseDto
                {
                    Success = false,
                    Message = "Failed to create patient in OpenMRS"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating patient in OpenMRS for PatientId: {PatientId}", patientId);
                return new OpenMrsPatientResponseDto
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        private async Task<string> GetNextIdentifierAsync()
        {
            try
            {
                _logger.LogInformation("Requesting next identifier from OpenMRS");

                var requestUrl = $"{OPENMRS_BASE_URL}/idgen/nextIdentifier?source=1";
                _logger.LogInformation("Making request to: {RequestUrl}", requestUrl);

                var response = await _httpClient.GetAsync(requestUrl);

                _logger.LogInformation("Response status: {StatusCode}", response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("Identifier API response: {Response}", jsonContent);

                    var identifierResponse = JsonSerializer.Deserialize<OpenMrsIdentifierResponseDto>(
                        jsonContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (identifierResponse?.Results?.Any() == true &&
                        !string.IsNullOrEmpty(identifierResponse.Results.First().IdentifierValue))
                    {
                        var identifier = identifierResponse.Results.First().IdentifierValue;
                        _logger.LogInformation("Successfully retrieved identifier: {Identifier}", identifier);
                        return identifier;
                    }

                    _logger.LogError("Identifier response was null or empty. Response: {JsonContent}", jsonContent);
                    return null;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to get identifier from OpenMRS. Status: {StatusCode}, Content: {Content}",
                    response.StatusCode, errorContent);
                return null;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP Request failed - possibly OpenMRS server not accessible");
                return null;
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "Failed to deserialize OpenMRS identifier response");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting identifier from OpenMRS");
                return null;
            }
        }

        private async Task<OpenMrsPatientCreatedResponseDto> CreatePatientAsync(OpenMrsPatientRequestDto patientRequest)
        {
            try
            {
                var jsonContent = JsonSerializer.Serialize(patientRequest, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true // For better logging
                });

                _logger.LogDebug("Creating patient in OpenMRS with data: {PatientData}", jsonContent);

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{OPENMRS_BASE_URL}/patient", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("Patient creation response: {Response}", responseContent);

                    var createdPatient = JsonSerializer.Deserialize<OpenMrsPatientCreatedResponseDto>(
                        responseContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (createdPatient != null)
                    {
                        _logger.LogInformation("Successfully created patient in OpenMRS with UUID: {Uuid}", createdPatient.Uuid);
                        return createdPatient;
                    }

                    _logger.LogError("Failed to deserialize patient creation response");
                    return null;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to create patient in OpenMRS. Status: {StatusCode}, Content: {Content}",
                    response.StatusCode, errorContent);
                return null;
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "Failed to serialize/deserialize patient data");
                return null;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP Request failed while creating patient");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating patient in OpenMRS");
                return null;
            }
        }

        private OpenMrsPatientRequestDto MapToOpenMrsPatient(Patient patient, string identifier)
        {
            try
            {
                // Calculate age from DOB
                var age = DateTime.UtcNow.Year - patient.DOB.Year;
                if (DateTime.UtcNow.DayOfYear < patient.DOB.DayOfYear)
                    age--;

                // Format birthdate for OpenMRS (ISO 8601 format)
                var birthdate = patient.DOB.ToString("yyyy-MM-ddTHH:mm:ss.fffK");

                // Parse address for better city/postal code (you can enhance this logic)
                var (cityVillage, postalCode) = ParseAddress(patient.Address);

                var openMrsPatient = new OpenMrsPatientRequestDto
                {
                    Identifiers = new List<OpenMrsIdentifierDto>
                    {
                        new OpenMrsIdentifierDto
                        {
                            Identifier = identifier,
                            IdentifierType = IDENTIFIER_TYPE_UUID,
                            Location = new OpenMrsLocationDto
                            {
                                Uuid = LOCATION_UUID
                            },
                            Preferred = true
                        }
                    },
                    Person = new OpenMrsPersonDto
                    {
                        Gender = patient.Gender?.ToUpper() ?? "U", // Default to 'U' (Unknown) if null
                        Age = age,
                        Birthdate = birthdate,
                        BirthdateEstimated = false,
                        Dead = false,
                        DeathDate = null,
                        CauseOfDeath = null,
                        Names = new List<OpenMrsNameDto>
                        {
                            new OpenMrsNameDto
                            {
                                GivenName = patient.FirstName,
                                FamilyName = patient.LastName
                            }
                        },
                        Addresses = new List<OpenMrsAddressDto>
                        {
                            new OpenMrsAddressDto
                            {
                                Address1 = patient.Address,
                                CityVillage = cityVillage,
                                Country = "Sri Lanka",
                                PostalCode = postalCode
                            }
                        }
                    }
                };

                _logger.LogDebug("Mapped patient {PatientId} to OpenMRS format. Age: {Age}, Gender: {Gender}",
                    patient.PatientId, age, patient.Gender);

                return openMrsPatient;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error mapping patient {PatientId} to OpenMRS format", patient.PatientId);
                throw;
            }
        }

        private (string cityVillage, string postalCode) ParseAddress(string address)
        {
            // Simple address parsing logic - you can enhance this based on your address format
            // Default values for Sri Lanka
            var cityVillage = "Colombo";
            var postalCode = "00100";

            if (!string.IsNullOrEmpty(address))
            {
                // Try to extract postal code (assuming it's at the end and numeric)
                var parts = address.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 1)
                {
                    var lastPart = parts.Last().Trim();
                    if (int.TryParse(lastPart, out _) && lastPart.Length >= 4)
                    {
                        postalCode = lastPart;
                    }

                    // Try to get city from second last part
                    if (parts.Length > 2)
                    {
                        cityVillage = parts[parts.Length - 2].Trim();
                    }
                }
            }

            return (cityVillage, postalCode);
        }

        // Helper method to test OpenMRS connection
        private async Task<bool> TestOpenMrsConnectionAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{OPENMRS_BASE_URL}/session");
                _logger.LogInformation("OpenMRS connection test - Status: {StatusCode}", response.StatusCode);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to OpenMRS");
                return false;
            }
        }

        // Helper method to get available identifier sources
        private async Task<string> GetAvailableIdentifierSourcesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{OPENMRS_BASE_URL}/idgen/identifiersource");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Available identifier sources: {Sources}", content);
                    return content;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting identifier sources");
            }
            return null;
        }
    }
}