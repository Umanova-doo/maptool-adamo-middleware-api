using System.Net.Http.Json;
using MAP2ADAMOINT.Models.DTOs;
using Microsoft.Extensions.Logging;

namespace MAP2ADAMOINT.Services
{
    /// <summary>
    /// Simple API client to call the hosted MAP2ADAMO API
    /// Ivan: Drop this into your MapTool application and call these methods to insert data into Oracle
    /// </summary>
    public class MapToOracleApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MapToOracleApiClient> _logger;

        // TODO: Replace with your actual hosted API URL
        private const string API_BASE_URL = "https://YOUR-API-URL-HERE.com";  // ← CHANGE THIS

        public MapToOracleApiClient(HttpClient httpClient, ILogger<MapToOracleApiClient> logger)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(API_BASE_URL);
            _logger = logger;
        }

        /// <summary>
        /// 1. Create MAP_INITIAL record (molecule initial evaluation)
        /// POST /adamo/initial
        /// </summary>
        public async Task<ApiResponse?> CreateMapInitial(CreateMapInitialRequest request)
        {
            try
            {
                _logger.LogInformation("Creating MAP_INITIAL for {GrNumber}", request.GrNumber);

                var response = await _httpClient.PostAsJsonAsync("/adamo/initial", request);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
                    _logger.LogInformation("✓ MAP_INITIAL created: {GrNumber}", request.GrNumber);
                    return result;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("✗ Failed to create MAP_INITIAL: {StatusCode} - {Error}", 
                        response.StatusCode, error);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling /adamo/initial");
                return null;
            }
        }

        /// <summary>
        /// 2. Create MAP_SESSION record (evaluation session)
        /// POST /adamo/session
        /// </summary>
        public async Task<ApiResponse?> CreateMapSession(CreateMapSessionRequest request)
        {
            try
            {
                _logger.LogInformation("Creating MAP_SESSION for {Stage} / {Segment}", 
                    request.Stage, request.Segment);

                var response = await _httpClient.PostAsJsonAsync("/adamo/session", request);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
                    _logger.LogInformation("✓ MAP_SESSION created");
                    return result;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("✗ Failed to create MAP_SESSION: {StatusCode} - {Error}", 
                        response.StatusCode, error);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling /adamo/session");
                return null;
            }
        }

        /// <summary>
        /// 3. Create MAP_RESULT record (evaluation result for a session)
        /// POST /adamo/result
        /// </summary>
        public async Task<ApiResponse?> CreateMapResult(CreateMapResultRequest request)
        {
            try
            {
                _logger.LogInformation("Creating MAP_RESULT for session {SessionId}, GR {GrNumber}", 
                    request.SessionId, request.GrNumber);

                var response = await _httpClient.PostAsJsonAsync("/adamo/result", request);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
                    _logger.LogInformation("✓ MAP_RESULT created for {GrNumber}", request.GrNumber);
                    return result;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("✗ Failed to create MAP_RESULT: {StatusCode} - {Error}", 
                        response.StatusCode, error);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling /adamo/result");
                return null;
            }
        }

        /// <summary>
        /// 4. Create MAP_SESSION with multiple MAP_RESULT records (all-in-one)
        /// POST /adamo/session-with-results
        /// </summary>
        public async Task<ApiResponse?> CreateSessionWithResults(CreateSessionWithResultsRequest request)
        {
            try
            {
                _logger.LogInformation("Creating MAP_SESSION with {Count} results", 
                    request.Results?.Count ?? 0);

                var response = await _httpClient.PostAsJsonAsync("/adamo/session-with-results", request);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
                    _logger.LogInformation("✓ MAP_SESSION created with {Count} results", 
                        request.Results?.Count ?? 0);
                    return result;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("✗ Failed to create session with results: {StatusCode} - {Error}", 
                        response.StatusCode, error);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling /adamo/session-with-results");
                return null;
            }
        }

        /// <summary>
        /// Test health endpoint
        /// </summary>
        public async Task<bool> TestHealthCheck()
        {
            try
            {
                _logger.LogInformation("Testing API health...");
                
                var response = await _httpClient.GetAsync("/health");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("✓ API is healthy: {Content}", content);
                    return true;
                }
                else
                {
                    _logger.LogWarning("✗ API health check failed: {StatusCode}", response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking API health");
                return false;
            }
        }
    }

    /// <summary>
    /// Generic API response wrapper
    /// </summary>
    public class ApiResponse
    {
        public string? Status { get; set; }
        public string? Message { get; set; }
        public object? Data { get; set; }
        public List<string>? Errors { get; set; }
    }
}


