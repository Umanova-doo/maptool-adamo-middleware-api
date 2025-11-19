using System.Net.Http.Json;
using System.Text.Json;
using MAP2ADAMOINT.Models.DTOs;
using Microsoft.Extensions.Logging;

namespace MAP2ADAMOINT.Services
{
    /// <summary>
    /// Simple API client to call the hosted MAP2ADAMO API
    /// Ivan: Drop this into your MapTool application and call these methods
    /// </summary>
    public class SyncApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SyncApiClient> _logger;

        // TODO: Replace with your actual hosted API URL
        private const string API_BASE_URL = "https://YOUR-API-URL-HERE.com";  // ← CHANGE THIS

        public SyncApiClient(HttpClient httpClient, ILogger<SyncApiClient> logger)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(API_BASE_URL);
            _logger = logger;
        }

        /// <summary>
        /// Test the sync from MapTool to Oracle
        /// Example usage:
        ///   var client = new SyncApiClient(httpClient, logger);
        ///   var response = await client.TestSyncFromMapTool(modifiedAfter: DateTime.Now.AddDays(-1));
        /// </summary>
        public async Task<SyncResponse?> TestSyncFromMapTool(
            DateTime? modifiedAfter = null,
            int batchSize = 10,
            bool dryRun = true)
        {
            try
            {
                _logger.LogInformation("Testing sync from MapTool to Oracle...");

                var request = new SyncFromMapRequest
                {
                    ModifiedAfter = modifiedAfter ?? DateTime.Now.AddDays(-1),
                    BatchSize = batchSize,
                    DryRun = dryRun,
                    SkipExisting = true,
                    IncludeEvaluations = true
                };

                var response = await _httpClient.PostAsJsonAsync("/sync/from-map", request);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<SyncResponse>();
                    
                    _logger.LogInformation(
                        "✓ Sync test successful | Status: {Status}, Synced: {Synced}, Skipped: {Skipped}",
                        result?.Status, result?.RecordsSynced, result?.RecordsSkipped);
                    
                    return result;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("✗ API call failed: {StatusCode} - {Error}", 
                        response.StatusCode, error);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling sync API");
                return null;
            }
        }

        /// <summary>
        /// Test the sync from Oracle to MapTool
        /// </summary>
        public async Task<SyncResponse?> TestSyncFromOracle(
            string? stage = null,
            int batchSize = 10,
            bool dryRun = true)
        {
            try
            {
                _logger.LogInformation("Testing sync from Oracle to MapTool...");

                var request = new SyncFromAdamoRequest
                {
                    Stage = stage,
                    BatchSize = batchSize,
                    DryRun = dryRun,
                    SkipExisting = true,
                    IncludeResults = true
                };

                var response = await _httpClient.PostAsJsonAsync("/sync/from-adamo", request);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<SyncResponse>();
                    
                    _logger.LogInformation(
                        "✓ Sync test successful | Status: {Status}, Synced: {Synced}, Skipped: {Skipped}",
                        result?.Status, result?.RecordsSynced, result?.RecordsSkipped);
                    
                    return result;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("✗ API call failed: {StatusCode} - {Error}", 
                        response.StatusCode, error);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling sync API");
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
    /// Example usage - Ivan can put this in a controller or background service
    /// </summary>
    public class SyncApiClientExample
    {
        private readonly SyncApiClient _apiClient;
        private readonly ILogger<SyncApiClientExample> _logger;

        public SyncApiClientExample(SyncApiClient apiClient, ILogger<SyncApiClientExample> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        /// <summary>
        /// Example: Test syncing molecules modified in last 24 hours
        /// </summary>
        public async Task RunSimpleTest()
        {
            _logger.LogInformation("=== Starting API Test ===");

            // 1. Check if API is up
            var isHealthy = await _apiClient.TestHealthCheck();
            if (!isHealthy)
            {
                _logger.LogError("API is not responding. Check the URL.");
                return;
            }

            // 2. Test sync from MapTool (dry run)
            var response = await _apiClient.TestSyncFromMapTool(
                modifiedAfter: DateTime.Now.AddDays(-1),
                batchSize: 10,
                dryRun: true  // Safe test mode - no actual writes
            );

            if (response != null)
            {
                _logger.LogInformation("Test Results:");
                _logger.LogInformation("  Status: {Status}", response.Status);
                _logger.LogInformation("  Records Processed: {Count}", response.RecordsProcessed);
                _logger.LogInformation("  Would Sync: {Count}", response.RecordsSynced);
                _logger.LogInformation("  Would Skip: {Count}", response.RecordsSkipped);
                _logger.LogInformation("  Message: {Message}", response.Message);
            }

            _logger.LogInformation("=== API Test Complete ===");
        }
    }
}

