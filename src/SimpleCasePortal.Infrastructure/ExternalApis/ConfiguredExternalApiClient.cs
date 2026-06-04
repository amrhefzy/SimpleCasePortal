using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SimpleCasePortal.Application.DTOs.Sync;
using SimpleCasePortal.Application.Interfaces;
using SimpleCasePortal.Domain.Enums;

namespace SimpleCasePortal.Infrastructure.ExternalApis;

public abstract class ConfiguredExternalApiClient : IExternalApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ExternalApisOptions _options;

    protected ConfiguredExternalApiClient(IOptions<ExternalApisOptions> options)
    {
        _options = options.Value;
    }

    public abstract SyncTargetEnum SyncTarget { get; }

    protected abstract ExternalApiTargetOptions TargetOptions { get; }

    public async Task<ExternalApiSyncResultDto> SendCaseAsync(
        ExternalCaseSyncPayloadDto payload,
        CancellationToken cancellationToken = default)
    {
        var targetOptions = TargetOptions;
        if (string.IsNullOrWhiteSpace(targetOptions.BaseUrl) ||
            string.IsNullOrWhiteSpace(targetOptions.Endpoint) ||
            string.IsNullOrWhiteSpace(targetOptions.ApiKey))
        {
            return new ExternalApiSyncResultDto
            {
                Success = false,
                ErrorCode = "MISSING_CONFIGURATION",
                Message = $"{SyncTarget} API is not configured."
            };
        }

        if (!Uri.TryCreate(targetOptions.BaseUrl, UriKind.Absolute, out var baseUri))
        {
            return new ExternalApiSyncResultDto
            {
                Success = false,
                ErrorCode = "INVALID_CONFIGURATION",
                Message = $"{SyncTarget} API base URL is invalid."
            };
        }

        var endpoint = targetOptions.Endpoint.StartsWith("/", StringComparison.Ordinal)
            ? targetOptions.Endpoint[1..]
            : targetOptions.Endpoint;
        var requestUri = new Uri(baseUri, endpoint);
        using var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(Math.Max(1, targetOptions.TimeoutSeconds))
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", targetOptions.ApiKey);

        try
        {
            using var response = await httpClient.SendAsync(request, cancellationToken);
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new ExternalApiSyncResultDto
                {
                    Success = false,
                    ErrorCode = $"HTTP_{(int)response.StatusCode}",
                    Message = CreateFailureMessage(response.StatusCode, responseText),
                    StatusCode = (int)response.StatusCode,
                    ResponsePayload = SanitizeResponsePayload(responseText)
                };
            }

            return ParseSuccessResponse(responseText, (int)response.StatusCode);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new ExternalApiSyncResultDto
            {
                Success = false,
                ErrorCode = "TIMEOUT",
                Message = $"{SyncTarget} API request timed out."
            };
        }
        catch (HttpRequestException ex)
        {
            return new ExternalApiSyncResultDto
            {
                Success = false,
                ErrorCode = "NETWORK_ERROR",
                Message = $"{SyncTarget} API request failed: {ex.Message}"
            };
        }
    }

    private static ExternalApiSyncResultDto ParseSuccessResponse(string responseText, int statusCode)
    {
        try
        {
            var response = JsonSerializer.Deserialize<ExternalApiSyncResultDto>(responseText, JsonOptions);
            if (response is not null)
            {
                response.Success = true;
                response.StatusCode = statusCode;
                response.ResponsePayload = SanitizeResponsePayload(responseText);
                response.Message = string.IsNullOrWhiteSpace(response.Message)
                    ? "External sync completed successfully."
                    : response.Message;
                return response;
            }
        }
        catch (JsonException)
        {
        }

        return new ExternalApiSyncResultDto
        {
            Success = true,
            Message = "External sync completed successfully.",
            StatusCode = statusCode,
            ResponsePayload = SanitizeResponsePayload(responseText)
        };
    }

    private static string CreateFailureMessage(HttpStatusCode statusCode, string responseText)
    {
        return string.IsNullOrWhiteSpace(responseText)
            ? $"External API returned {(int)statusCode} {statusCode}."
            : $"External API returned {(int)statusCode} {statusCode}.";
    }

    private static string? SanitizeResponsePayload(string responseText)
    {
        return string.IsNullOrWhiteSpace(responseText) ? null : responseText;
    }

    protected ExternalApiTargetOptions GetTargetOptions()
    {
        return SyncTarget switch
        {
            SyncTargetEnum.DentistApp => _options.DentistApp,
            SyncTargetEnum.WorkflowApp => _options.WorkflowApp,
            SyncTargetEnum.ProductionApp => _options.ProductionApp,
            _ => throw new InvalidOperationException("Unsupported sync target.")
        };
    }
}
