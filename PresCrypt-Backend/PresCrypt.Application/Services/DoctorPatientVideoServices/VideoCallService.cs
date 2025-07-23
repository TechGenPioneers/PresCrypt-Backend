using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PresCrypt_Backend.PresCrypt.API.Dto;

namespace PresCrypt_Backend.PresCrypt.Application.Services.DoctorPatientVideoServices
{
    public class VideoCallService : IVideoCallService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ApplicationDbContext _context;

        public VideoCallService(
            HttpClient httpClient,
            IConfiguration configuration,
            ApplicationDbContext context)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Whereby:ApiKey"];
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);
            _context = context;
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

        public async Task<DoctorPatientVideoCallDto> GetDoctorNameAsync(string doctorId)
        {
            var doctorName = await _context.Doctor
                .AsNoTracking()
                .Where(d => d.DoctorId == doctorId)
                .Select(d => new DoctorPatientVideoCallDto(d.FirstName, d.LastName))
                .FirstOrDefaultAsync();

            if (doctorName == null)
                throw new KeyNotFoundException($"Doctor with ID {doctorId} not found");

            return doctorName;
        }

        public async Task<DoctorPatientVideoCallDto> GetPatientNameAsync(string patientId)
        {
            var patientName = await _context.Patient
                .AsNoTracking()
                .Where(p => p.PatientId == patientId)
                .Select(p => new DoctorPatientVideoCallDto(p.FirstName, p.LastName))
                .FirstOrDefaultAsync();

            if (patientName == null)
                throw new KeyNotFoundException($"Patient with ID {patientId} not found");

            return patientName;
        }
    }
}
