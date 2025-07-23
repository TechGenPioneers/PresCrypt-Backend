using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace PresCrypt_Backend.PresCrypt.Application.Services.OpenMrsServices
{

    public class OpenMrsAttachmentService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OpenMrsAttachmentService> _logger;
        private readonly string _baseUrl;
        private readonly string _conceptUuid = "42ed45fd-f3f6-44b6-bfc2-8bde1bb41e00";

        public OpenMrsAttachmentService(
            HttpClient httpClient,
            ILogger<OpenMrsAttachmentService> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _baseUrl = configuration["OpenMRS:BaseUrl"] ?? "http://localhost:80/openmrs";

            // ✅ Add Basic Authentication for OpenMRS
            var username = "admin";
            var password = "Admin123";
            var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            // Optionally accept JSON
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<ObservationWithAttachmentResponse> GetObservationWithAttachmentAsync(string openMrsId)
        {
            try
            {
                var observationsUrl = $"{_baseUrl}/ws/rest/v1/obs?patient={openMrsId}&concept={_conceptUuid}";
                _logger.LogInformation($"Fetching observations from: {observationsUrl}");

                var observationsResponse = await _httpClient.GetAsync(observationsUrl);

                if (!observationsResponse.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to fetch observations. Status: {observationsResponse.StatusCode}");
                    return new ObservationWithAttachmentResponse
                    {
                        Success = false,
                        ErrorMessage = $"Failed to fetch observations: {observationsResponse.StatusCode}"
                    };
                }

                var observationsJson = await observationsResponse.Content.ReadAsStringAsync();
                var observationsData = JsonSerializer.Deserialize<ObservationsResponse>(observationsJson);

                if (observationsData?.Results == null || observationsData.Results.Count == 0)
                {
                    _logger.LogWarning($"No observations found for patient: {openMrsId}");
                    return new ObservationWithAttachmentResponse
                    {
                        Success = false,
                        ErrorMessage = "No observations found for the patient"
                    };
                }

                var observationsWithAttachments = new List<ObservationWithAttachment>();

                foreach (var observation in observationsData.Results)
                {
                    var attachmentUrl = $"{_baseUrl}/ws/rest/v1/obs/{observation.Uuid}/value";
                    _logger.LogInformation($"Fetching attachment from: {attachmentUrl}");

                    var attachmentResponse = await _httpClient.GetAsync(attachmentUrl);

                    byte[]? attachmentValue = null;
                    if (attachmentResponse.IsSuccessStatusCode)
                    {
                        attachmentValue = await attachmentResponse.Content.ReadAsByteArrayAsync();
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to fetch attachment for observation {observation.Uuid}. Status: {attachmentResponse.StatusCode}");
                    }

                    observationsWithAttachments.Add(new ObservationWithAttachment
                    {
                        ObservationUuid = observation.Uuid,
                        ConceptUuid = _conceptUuid,
                        PatientUuid = openMrsId,
                        ObsDatetime = observation.ObsDatetime,
                        Value = observation.Value,
                        AttachmentValue = attachmentValue,
                        Comment = observation.Comment,
                        Location = observation.Location,
                        Encounter = observation.Encounter,
                        Creator = observation.Creator,
                        DateCreated = observation.DateCreated
                    });
                }

                return new ObservationWithAttachmentResponse
                {
                    Success = true,
                    ConceptUuid = _conceptUuid,
                    PatientUuid = openMrsId,
                    Observations = observationsWithAttachments,
                    TotalCount = observationsWithAttachments.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching observation with attachment for patient: {openMrsId}");
                return new ObservationWithAttachmentResponse
                {
                    Success = false,
                    ErrorMessage = $"An error occurred: {ex.Message}"
                };
            }
        }

        public async Task<ObservationWithAttachmentResponse> GetSingleObservationWithAttachmentAsync(string openMrsId, string observationUuid)
        {
            try
            {
                var observationUrl = $"{_baseUrl}/ws/rest/v1/obs/{observationUuid}";
                _logger.LogInformation($"Fetching observation from: {observationUrl}");

                var observationResponse = await _httpClient.GetAsync(observationUrl);

                if (!observationResponse.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to fetch observation. Status: {observationResponse.StatusCode}");
                    return new ObservationWithAttachmentResponse
                    {
                        Success = false,
                        ErrorMessage = $"Failed to fetch observation: {observationResponse.StatusCode}"
                    };
                }

                var observationJson = await observationResponse.Content.ReadAsStringAsync();
                var observation = JsonSerializer.Deserialize<Observation>(observationJson);

                if (observation == null)
                {
                    return new ObservationWithAttachmentResponse
                    {
                        Success = false,
                        ErrorMessage = "Observation not found"
                    };
                }

                var attachmentUrl = $"{_baseUrl}/ws/rest/v1/obs/{observationUuid}/value";
                _logger.LogInformation($"Fetching attachment from: {attachmentUrl}");

                var attachmentResponse = await _httpClient.GetAsync(attachmentUrl);

                byte[]? attachmentValue = null;
                if (attachmentResponse.IsSuccessStatusCode)
                {
                    attachmentValue = await attachmentResponse.Content.ReadAsByteArrayAsync();
                }
                else
                {
                    _logger.LogWarning($"Failed to fetch attachment for observation {observationUuid}. Status: {attachmentResponse.StatusCode}");
                }

                var observationWithAttachment = new ObservationWithAttachment
                {
                    ObservationUuid = observation.Uuid,
                    ConceptUuid = _conceptUuid,
                    PatientUuid = openMrsId,
                    ObsDatetime = observation.ObsDatetime,
                    Value = observation.Value,
                    AttachmentValue = attachmentValue,
                    Comment = observation.Comment,
                    Location = observation.Location,
                    Encounter = observation.Encounter,
                    Creator = observation.Creator,
                    DateCreated = observation.DateCreated
                };

                return new ObservationWithAttachmentResponse
                {
                    Success = true,
                    ConceptUuid = _conceptUuid,
                    PatientUuid = openMrsId,
                    Observations = new List<ObservationWithAttachment> { observationWithAttachment },
                    TotalCount = 1
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching single observation with attachment: {observationUuid}");
                return new ObservationWithAttachmentResponse
                {
                    Success = false,
                    ErrorMessage = $"An error occurred: {ex.Message}"
                };
            }
        }
    }

    // Response Models
    public class ObservationWithAttachmentResponse
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string ConceptUuid { get; set; } = string.Empty;
        public string PatientUuid { get; set; } = string.Empty;
        public List<ObservationWithAttachment> Observations { get; set; } = new();
        public int TotalCount { get; set; }
    }

    public class ObservationWithAttachment
    {
        public string ObservationUuid { get; set; } = string.Empty;
        public string ConceptUuid { get; set; } = string.Empty;
        public string PatientUuid { get; set; } = string.Empty;
        public DateTime? ObsDatetime { get; set; }
        public object? Value { get; set; }
        public byte[]? AttachmentValue { get; set; }
        public string? Comment { get; set; }
        public object? Location { get; set; }
        public object? Encounter { get; set; }
        public object? Creator { get; set; }
        public DateTime? DateCreated { get; set; }
    }

    public class ObservationsResponse
    {
        [JsonPropertyName("results")]
        public List<Observation> Results { get; set; } = new();
    }

    public class Observation
    {
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; } = string.Empty;

        [JsonPropertyName("concept")]
        public object? Concept { get; set; }

        [JsonPropertyName("person")]
        public object? Person { get; set; }

        [JsonPropertyName("obsDatetime")]
        public DateTime? ObsDatetime { get; set; }

        [JsonPropertyName("value")]
        public object? Value { get; set; }

        [JsonPropertyName("comment")]
        public string? Comment { get; set; }

        [JsonPropertyName("location")]
        public object? Location { get; set; }

        [JsonPropertyName("encounter")]
        public object? Encounter { get; set; }

        [JsonPropertyName("creator")]
        public object? Creator { get; set; }

        [JsonPropertyName("dateCreated")]
        public DateTime? DateCreated { get; set; }
    }
}
