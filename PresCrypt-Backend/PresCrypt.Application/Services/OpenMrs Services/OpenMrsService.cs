using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PresCrypt_Backend.PresCrypt.Application.Services.OpenMrs_Services
{
    public interface IOpenMrsService
    {
        Task<JsonDocument> GetOpenMrsData(string resourceType, string resourceId);
    }

    public class OpenMrsService : IOpenMrsService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OpenMrsService> _logger;

        public OpenMrsService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<OpenMrsService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            // Configure the base URL from configuration
            _httpClient.BaseAddress = new Uri(_configuration["OpenMrs:BaseUrl"]);

            // Configure basic authentication if needed
            string username = _configuration["OpenMrs:Username"];
            string password = _configuration["OpenMrs:Password"];
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                string authValue = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{username}:{password}"));
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authValue);
            }
        }

        public async Task<JsonDocument> GetOpenMrsData(string resourceType, string resourceId)
        {
            try
            {
                // Example: /ws/rest/v1/obs?patient=UUID&v=full
                string requestUrl = $"/ws/rest/v1/{resourceType}?patient={resourceId}&v=full";
                _logger.LogInformation($"Requesting OpenMRS data: {requestUrl}");

                var response = await _httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();

                string content = await response.Content.ReadAsStringAsync();
                return JsonDocument.Parse(content);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Error fetching {resourceType} data for {resourceId} from OpenMRS");
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, $"Error parsing JSON response from OpenMRS for {resourceType}/{resourceId}");
                throw;
            }
        }
    }
}
