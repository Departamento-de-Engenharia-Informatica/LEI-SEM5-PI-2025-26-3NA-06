using System.Net.Http.Headers;
using System.Text.Json;
using ProjArqsi.SchedulingApi.DTOs;
using ProjArqsi.SchedulingApi.Logging;

namespace ProjArqsi.SchedulingApi.Services
{
    public interface ICoreApiClientService
    {
        Task<List<VesselVisitNotificationDto>> GetApprovedVVNsForDateAsync(DateTime date, string accessToken);
        Task<List<DockDto>> GetAllDocksAsync(string accessToken);
        Task<DockDto?> GetDockByIdAsync(Guid dockId, string accessToken);
        Task<List<StorageAreaDto>> GetAllStorageAreasAsync(string accessToken);
        Task<VesselDto?> GetVesselByImoAsync(string imo, string accessToken);
    }

    public class CoreApiClientService : ICoreApiClientService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public CoreApiClientService(HttpClient httpClient, ISchedulingLogger schedulingLogger)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<List<VesselVisitNotificationDto>> GetApprovedVVNsForDateAsync(DateTime date, string accessToken)    
        {
            if (string.IsNullOrEmpty(accessToken))
                throw new UnauthorizedAccessException("Access token is required");
            try
            {
                var endpoint = $"/api/VesselVisitNotification/approved?date={date:yyyy-MM-dd}";
                Console.WriteLine($"[CoreApiClient] Calling Core API: {endpoint}");
                
                var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                
                Console.WriteLine($"[CoreApiClient] Response Status: {response.StatusCode}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[CoreApiClient] ERROR - Status {response.StatusCode}: {errorContent}");
                    return new List<VesselVisitNotificationDto>();
                }

                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[CoreApiClient] Response Content Length: {content.Length}");
                Console.WriteLine($"[CoreApiClient] Response Content: {content}");
                
                var vvns = JsonSerializer.Deserialize<List<VesselVisitNotificationDto>>(content, _jsonOptions);
                Console.WriteLine($"[CoreApiClient] Deserialized {vvns?.Count ?? 0} VVNs");
                
                return vvns ?? new List<VesselVisitNotificationDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CoreApiClient] EXCEPTION: {ex.Message}");
                Console.WriteLine($"[CoreApiClient] Stack Trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<DockDto?> GetDockByIdAsync(Guid dockId, string accessToken)
        {
             if (string.IsNullOrEmpty(accessToken))
                throw new UnauthorizedAccessException("Access token is required");
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"/api/Dock/{dockId}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<DockDto>(content, _jsonOptions);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<VesselDto?> GetVesselByImoAsync(string imo, string accessToken)
        {
             if (string.IsNullOrEmpty(accessToken))
                throw new UnauthorizedAccessException("Access token is required");
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"/api/Vessel/imo/{imo}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<VesselDto>(content, _jsonOptions);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<DockDto>> GetAllDocksAsync(string accessToken)
        {
             if (string.IsNullOrEmpty(accessToken))
                throw new UnauthorizedAccessException("Access token is required");
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "/api/Dock");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    return new List<DockDto>();
                }

                var content = await response.Content.ReadAsStringAsync();
                var docks = JsonSerializer.Deserialize<List<DockDto>>(content, _jsonOptions);
                
                return docks ?? new List<DockDto>();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<StorageAreaDto>> GetAllStorageAreasAsync(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
                throw new UnauthorizedAccessException("Access token is required");
        
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "/api/StorageArea");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    return new List<StorageAreaDto>();
                }

                var content = await response.Content.ReadAsStringAsync();
                var areas = JsonSerializer.Deserialize<List<StorageAreaDto>>(content, _jsonOptions);
                
                return areas ?? new List<StorageAreaDto>();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
