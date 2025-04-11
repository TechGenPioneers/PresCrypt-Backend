using Microsoft.EntityFrameworkCore;
using PresCrypt_Backend.PresCrypt.Core.Models;
using PresCrypt_Backend.PresCrypt.API.Dto;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json; // for serializing and deserializing data.
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PresCrypt_Backend.PresCrypt.Application.Services.DoctorPrescriptionServices
{
    public class DoctorPrescriptionSubmitService : IDoctorPrescriptionSubmitService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DoctorPrescriptionSubmitService> _logger;
        private readonly OpenMrsConfig _openMrsConfig;
        //private readonly IHospitalMappingService _hospitalMappingService;

        public DoctorPrescriptionSubmitService(
            HttpClient httpClient,
            ILogger<DoctorPrescriptionSubmitService> logger,
            IOptions<OpenMrsConfig> openMrsConfig
            //IHospitalMappingService hospitalMappingService
            )
        {
            _httpClient = httpClient;
            _logger = logger;
            _openMrsConfig = openMrsConfig.Value;
            //_hospitalMappingService = hospitalMappingService;
        }

        public async Task<string> SubmitPrescriptionAsync(DoctorPrescriptionDto dto)
        {
            if (dto.PrescriptionFile != null)
            {
                var fileUrl = await UploadFileToOpenMRS(dto.PrescriptionFile);
                dto.PrescriptionText = fileUrl;
                return "Prescription file uploaded successfully.";
            }

            if (!string.IsNullOrWhiteSpace(dto.PrescriptionText))
            {
                return $"Prescription processed: {dto.PrescriptionText.Truncate(30)}";
            }

            throw new ArgumentException("No prescription content provided.");
        }

        public async Task<bool> SendToOpenMRSAsync(DoctorPrescriptionDto dto)
        {
            try
            {
                var patientUuid = await GetPatientUuid(dto.PatientId);
                if (string.IsNullOrEmpty(patientUuid))
                    throw new KeyNotFoundException($"Patient {dto.PatientId} not found in OpenMRS");

                //var locationUuid = await _hospitalMappingService.GetLocationUuid(dto.HospitalId);
                var encounterTypeUuid = _openMrsConfig.DefaultEncounterTypeUuid;

                var encounter = new OpenMrsEncounter
                {
                    Patient = patientUuid,
                    EncounterType = encounterTypeUuid,
                    //Location = locationUuid,
                    EncounterDatetime = DateTime.UtcNow,
                    Obs = new List<OpenMrsObservation>
                    {
                        new()
                        {
                            Concept = _openMrsConfig.PrescriptionConceptUuid,
                            Value = dto.PrescriptionText ?? "Prescription document"
                        }
                    }
                };

                var response = await _httpClient.PostAsJsonAsync(
                    $"{_openMrsConfig.BaseUrl}/ws/rest/v1/encounter",
                    encounter);

                if (!response.IsSuccessStatusCode)
                {
                    var errorDetails = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error sending to OpenMRS: {errorDetails}");
                    return false;
                }

                return true;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error while connecting to OpenMRS");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OpenMRS integration error");
                return false;
            }
        }

        //get patient uuid
        private async Task<string> GetPatientUuid(string patientId)
        {
            var response = await _httpClient.GetAsync(
                $"{_openMrsConfig.BaseUrl}/ws/rest/v1/patient?identifier={patientId}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Patient not found for {patientId}");
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<OpenMrsPatientSearchResult>();
            return result?.Results.FirstOrDefault()?.Uuid;
        }

        private async Task<string> UploadFileToOpenMRS(IFormFile file)
        {
            using var content = new MultipartFormDataContent();
            using var fileStream = file.OpenReadStream();
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);

            content.Add(fileContent, "file", file.FileName);

            var response = await _httpClient.PostAsync(
                $"{_openMrsConfig.BaseUrl}/ws/rest/v1/attachment",
                content);

            if (!response.IsSuccessStatusCode)
            {
                var errorDetails = await response.Content.ReadAsStringAsync();
                _logger.LogError($"File upload failed: {errorDetails}");
                response.EnsureSuccessStatusCode();
            }

            var result = await response.Content.ReadFromJsonAsync<OpenMrsAttachmentResponse>();
            return result?.Links?.Uri?.Href;
        }
    }

    // Configuration Models
    public class OpenMrsConfig
    {
        public string BaseUrl { get; set; }
        public string DefaultEncounterTypeUuid { get; set; }
        public string PrescriptionConceptUuid { get; set; }
    }

    public class OpenMrsEncounter
    {
        public string Patient { get; set; }
        public string EncounterType { get; set; }
        public string Location { get; set; }
        public DateTime EncounterDatetime { get; set; }
        public List<OpenMrsObservation> Obs { get; set; }
    }

    public class OpenMrsObservation
    {
        public string Concept { get; set; }
        public string Value { get; set; }
    }

    // Classes for OpenMRS Response
    public class OpenMrsPatientSearchResult
    {
        public List<OpenMrsPatientResult> Results { get; set; }
    }

    public class OpenMrsPatientResult
    {
        public string Uuid { get; set; }
    }

    public class OpenMrsAttachmentResponse
    {
        public OpenMrsAttachmentLinks Links { get; set; }
    }

    public class OpenMrsAttachmentLinks
    {
        public OpenMrsAttachmentUri Uri { get; set; }
    }

    public class OpenMrsAttachmentUri
    {
        public string Href { get; set; }  // The URL of the uploaded file.
    }

    // Extension Method for String
    public static class StringExtensions
    {
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...";
        }
    }
}
