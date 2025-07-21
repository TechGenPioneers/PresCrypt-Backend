using System.Net.Http.Headers;
using System.Text;

namespace PresCrypt_Backend.PresCrypt.Application.Services.OpenMrsServices
{

    public class OpenMrsObsService : IOpenMrsObsService
    {
        private readonly HttpClient _httpClient;

        public OpenMrsObsService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            // Add basic auth header
            var byteArray = Encoding.ASCII.GetBytes("admin:Admin123");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }

        public async Task<string> GetObservationsByPatientIdAsync(string openMrsId)
        {
            var url = $"http://localhost:80/openmrs/ws/rest/v1/obs?patient={openMrsId}";
            var response = await _httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
