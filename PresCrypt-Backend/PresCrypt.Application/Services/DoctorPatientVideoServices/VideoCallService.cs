using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using PresCrypt_Backend.PresCrypt.Infrastructure.Repositories;
using PresCrypt_Backend.PresCrypt.Core.Models;

namespace PresCrypt_Backend.PresCrypt.Application.Services.DoctorPatientVideoServices
{
    public class VideoCallService : IVideoCallService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IPatientRepository _patientRepository;

        public VideoCallService(
            HttpClient httpClient,
            IConfiguration configuration,
            IDoctorRepository doctorRepository,
            IPatientRepository patientRepository)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Whereby:ApiKey"];
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
        }

        public async Task<string> CreateRoomAsync(string roomName)
        {
            var payload = new
            {
                endDate = DateTime.UtcNow.AddHours(1).ToString("o"),
                roomNamePrefix = roomName,
                isLocked = false
            };

            var jsonPayload = JsonSerializer.Serialize(payload);

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.whereby.dev/v1/meetings");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Dictionary<string, object>>(responseJson);

            if (result != null && result.TryGetValue("roomUrl", out var roomUrlObj))
            {
                return roomUrlObj?.ToString() ?? throw new Exception("roomUrl not found.");
            }

            throw new Exception("Failed to create room.");
        }

        public Task<string> GetRoomAsync(string roomId)
        {
            throw new NotSupportedException("Whereby API does not support retrieving rooms by ID.");
        }

        public Task<string> GenerateAccessUrlAsync(string roomId, string userRole)
        {
            return Task.FromResult($"https://prescrypt-telehealth.whereby.com/{roomId}");
        }

        public async Task<string> GetDoctorNameAsync(string doctorId)
        {
            var doctor = await _doctorRepository.GetByIdAsync(doctorId);
            if (doctor == null)
            {
                throw new KeyNotFoundException($"Doctor with ID {doctorId} not found");
            }
            return $"{doctor.FirstName} {doctor.LastName}";
        }

        public async Task<string> GetPatientNameAsync(string patientId)
        {
            var patient = await _patientRepository.GetByIdAsync(patientId);
            if (patient == null)
            {
                throw new KeyNotFoundException($"Patient with ID {patientId} not found");
            }
            return $"{patient.FirstName} {patient.LastName}";
        }
    }
}
