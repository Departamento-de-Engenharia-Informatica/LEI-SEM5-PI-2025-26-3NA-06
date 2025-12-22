using System.Net.Http.Headers;
using System.Text.Json;
using ProjArqsi.SchedulingApi.DTOs;
using ProjArqsi.SchedulingApi.Logging;

namespace ProjArqsi.SchedulingApi.Services
{
    public interface ICoreApiClient
    {
        Task<List<VesselVisitNotificationDto>> GetApprovedVVNsForDateAsync(DateTime date, string accessToken);
        Task<List<DockDto>> GetAllDocksAsync(string accessToken);
        Task<DockDto?> GetDockByIdAsync(Guid dockId, string accessToken);
        Task<List<StorageAreaDto>> GetAllStorageAreasAsync(string accessToken);
        Task<VesselDto?> GetVesselByImoAsync(string imo, string accessToken);
    }

    public class CoreApiClientService : ICoreApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ISchedulingLogger _schedulingLogger;
        private readonly JsonSerializerOptions _jsonOptions;

        public CoreApiClientService(HttpClient httpClient, ISchedulingLogger schedulingLogger)
        {
            _httpClient = httpClient;
            _schedulingLogger = schedulingLogger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<List<VesselVisitNotificationDto>> GetApprovedVVNsForDateAsync(DateTime date, string accessToken)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, 
                    $"/api/VesselVisitNotification/approved?date={date:yyyy-MM-dd}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    _schedulingLogger.LogCoreApiCallFailed("/api/VesselVisitNotification/approved", (int)response.StatusCode, $"Failed to fetch approved VVNs");
                    return new List<VesselVisitNotificationDto>();
                }

                var content = await response.Content.ReadAsStringAsync();
                var vvns = JsonSerializer.Deserialize<List<VesselVisitNotificationDto>>(content, _jsonOptions);
                
                return vvns ?? new List<VesselVisitNotificationDto>();
            }
            catch (Exception ex)
            {
                _schedulingLogger.LogCoreApiException("/api/VesselVisitNotification/approved", ex);
                throw;
            }
        }

        public async Task<DockDto?> GetDockByIdAsync(Guid dockId, string accessToken)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"/api/Dock/{dockId}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    _schedulingLogger.LogCoreApiCallFailed($"/api/Dock/{dockId}", (int)response.StatusCode, $"Failed to fetch dock {dockId}");
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<DockDto>(content, _jsonOptions);
            }
            catch (Exception ex)
            {
                _schedulingLogger.LogCoreApiException($"/api/Dock/{dockId}", ex);
                return null;
            }
        }

        public async Task<VesselDto?> GetVesselByImoAsync(string imo, string accessToken)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"/api/Vessel/imo/{imo}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    _schedulingLogger.LogCoreApiCallFailed($"/api/Vessel/imo/{imo}", (int)response.StatusCode, $"Failed to fetch vessel {imo}");
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<VesselDto>(content, _jsonOptions);
            }
            catch (Exception ex)
            {
                _schedulingLogger.LogCoreApiException($"/api/Vessel/imo/{imo}", ex);
                return null;
            }
        }

        public async Task<List<DockDto>> GetAllDocksAsync(string accessToken)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "/api/Dock");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    _schedulingLogger.LogCoreApiCallFailed("/api/Dock", (int)response.StatusCode, "Failed to fetch all docks");
                    return new List<DockDto>();
                }

                var content = await response.Content.ReadAsStringAsync();
                var docks = JsonSerializer.Deserialize<List<DockDto>>(content, _jsonOptions);
                
                return docks ?? new List<DockDto>();
            }
            catch (Exception ex)
            {
                _schedulingLogger.LogCoreApiException("/api/Dock", ex);
                throw;
            }
        }

        public async Task<List<StorageAreaDto>> GetAllStorageAreasAsync(string accessToken)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "/api/StorageArea");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    _schedulingLogger.LogCoreApiCallFailed("/api/StorageArea", (int)response.StatusCode, "Failed to fetch all storage areas");
                    return new List<StorageAreaDto>();
                }

                var content = await response.Content.ReadAsStringAsync();
                var areas = JsonSerializer.Deserialize<List<StorageAreaDto>>(content, _jsonOptions);
                
                return areas ?? new List<StorageAreaDto>();
            }
            catch (Exception ex)
            {
                _schedulingLogger.LogCoreApiException("/api/StorageArea", ex);
                throw;
            }
        }
    }
}
