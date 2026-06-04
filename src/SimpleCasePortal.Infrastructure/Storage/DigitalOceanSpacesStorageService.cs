using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using SimpleCasePortal.Application.DTOs.Files;
using SimpleCasePortal.Application.Interfaces;

namespace SimpleCasePortal.Infrastructure.Storage;

public sealed class DigitalOceanSpacesStorageService : IFileStorageService
{
    private const string ServiceName = "s3";
    private const string Algorithm = "AWS4-HMAC-SHA256";
    private readonly StorageOptions _options;
    private static readonly HttpClient HttpClient = new();

    public DigitalOceanSpacesStorageService(IOptions<StorageOptions> options)
    {
        _options = options.Value;
    }

    public async Task<FileUploadResultDto> UploadAsync(StorageUploadRequestDto request, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        var uri = BuildObjectUri(request.ObjectKey);
        var payloadHash = await ComputeSha256HexAsync(request.Content, cancellationToken);
        request.Content.Position = 0;

        using var httpRequest = new HttpRequestMessage(HttpMethod.Put, uri)
        {
            Content = new StreamContent(request.Content)
        };
        httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue(request.ContentType);
        httpRequest.Content.Headers.ContentLength = request.FileSizeBytes;
        SignHeaderRequest(httpRequest, payloadHash);

        using var response = await HttpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        return new FileUploadResultDto
        {
            ObjectKey = request.ObjectKey,
            StoredFileName = request.StoredFileName,
            FileSizeBytes = request.FileSizeBytes,
            ContentType = request.ContentType,
            Checksum = request.Checksum
        };
    }

    public Task<SignedFileUrlDto> GenerateSignedDownloadUrlAsync(int fileId, string objectKey, string fileName, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        var expiresOnUtc = DateTime.UtcNow.AddMinutes(Math.Max(1, _options.SignedUrlExpiryMinutes));
        var url = BuildPresignedUrl(HttpMethod.Get.Method, objectKey, TimeSpan.FromMinutes(Math.Max(1, _options.SignedUrlExpiryMinutes)));

        return Task.FromResult(new SignedFileUrlDto
        {
            FileId = fileId,
            FileName = fileName,
            Url = url,
            ExpiresOnUtc = expiresOnUtc
        });
    }

    public async Task<bool> ObjectExistsAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        using var request = new HttpRequestMessage(HttpMethod.Head, BuildObjectUri(objectKey));
        SignHeaderRequest(request, "UNSIGNED-PAYLOAD");

        using var response = await HttpClient.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task DeleteAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        using var request = new HttpRequestMessage(HttpMethod.Delete, BuildObjectUri(objectKey));
        SignHeaderRequest(request, "UNSIGNED-PAYLOAD");

        using var response = await HttpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public Task<(Stream Content, string ContentType, string FileName)> OpenSignedDownloadAsync(string token, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Direct local signed downloads are not used for DigitalOcean Spaces.");
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_options.ServiceUrl) ||
            string.IsNullOrWhiteSpace(_options.Region) ||
            string.IsNullOrWhiteSpace(_options.BucketName) ||
            string.IsNullOrWhiteSpace(_options.AccessKey) ||
            string.IsNullOrWhiteSpace(_options.SecretKey))
        {
            throw new InvalidOperationException("DigitalOcean Spaces storage is not fully configured.");
        }
    }

    private Uri BuildObjectUri(string objectKey)
    {
        var encodedKey = string.Join("/", objectKey.Split('/').Select(Uri.EscapeDataString));
        return new Uri($"{_options.ServiceUrl!.TrimEnd('/')}/{_options.BucketName}/{encodedKey}");
    }

    private string BuildPresignedUrl(string method, string objectKey, TimeSpan expires)
    {
        var now = DateTime.UtcNow;
        var amzDate = now.ToString("yyyyMMdd'T'HHmmss'Z'", System.Globalization.CultureInfo.InvariantCulture);
        var dateStamp = now.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
        var credentialScope = $"{dateStamp}/{_options.Region}/{ServiceName}/aws4_request";
        var credential = $"{_options.AccessKey}/{credentialScope}";
        var uri = BuildObjectUri(objectKey);
        var host = uri.Host;
        var canonicalUri = uri.AbsolutePath;
        var query = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["X-Amz-Algorithm"] = Algorithm,
            ["X-Amz-Credential"] = credential,
            ["X-Amz-Date"] = amzDate,
            ["X-Amz-Expires"] = Math.Clamp((int)expires.TotalSeconds, 1, 604800).ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["X-Amz-SignedHeaders"] = "host"
        };

        var canonicalQuery = string.Join("&", query.Select(pair => $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value)}"));
        var canonicalRequest = string.Join('\n',
        [
            method,
            canonicalUri,
            canonicalQuery,
            $"host:{host}\n",
            "host",
            "UNSIGNED-PAYLOAD"
        ]);

        var stringToSign = string.Join('\n',
        [
            Algorithm,
            amzDate,
            credentialScope,
            ToHex(SHA256.HashData(Encoding.UTF8.GetBytes(canonicalRequest)))
        ]);

        var signature = ToHex(Hmac(GetSigningKey(dateStamp), stringToSign));
        return $"{uri}?{canonicalQuery}&X-Amz-Signature={signature}";
    }

    private void SignHeaderRequest(HttpRequestMessage request, string payloadHash)
    {
        var now = DateTime.UtcNow;
        var amzDate = now.ToString("yyyyMMdd'T'HHmmss'Z'", System.Globalization.CultureInfo.InvariantCulture);
        var dateStamp = now.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
        var credentialScope = $"{dateStamp}/{_options.Region}/{ServiceName}/aws4_request";
        var host = request.RequestUri!.Host;
        var signedHeaders = "host;x-amz-content-sha256;x-amz-date";

        request.Headers.Host = host;
        request.Headers.TryAddWithoutValidation("x-amz-content-sha256", payloadHash);
        request.Headers.TryAddWithoutValidation("x-amz-date", amzDate);

        var canonicalRequest = string.Join('\n',
        [
            request.Method.Method,
            request.RequestUri.AbsolutePath,
            string.Empty,
            $"host:{host}\nx-amz-content-sha256:{payloadHash}\nx-amz-date:{amzDate}\n",
            signedHeaders,
            payloadHash
        ]);

        var stringToSign = string.Join('\n',
        [
            Algorithm,
            amzDate,
            credentialScope,
            ToHex(SHA256.HashData(Encoding.UTF8.GetBytes(canonicalRequest)))
        ]);

        var signature = ToHex(Hmac(GetSigningKey(dateStamp), stringToSign));
        request.Headers.Authorization = AuthenticationHeaderValue.Parse(
            $"{Algorithm} Credential={_options.AccessKey}/{credentialScope}, SignedHeaders={signedHeaders}, Signature={signature}");
    }

    private static async Task<string> ComputeSha256HexAsync(Stream stream, CancellationToken cancellationToken)
    {
        var hash = await SHA256.HashDataAsync(stream, cancellationToken);
        return ToHex(hash);
    }

    private byte[] GetSigningKey(string dateStamp)
    {
        var dateKey = Hmac(Encoding.UTF8.GetBytes($"AWS4{_options.SecretKey}"), dateStamp);
        var regionKey = Hmac(dateKey, _options.Region!);
        var serviceKey = Hmac(regionKey, ServiceName);
        return Hmac(serviceKey, "aws4_request");
    }

    private static byte[] Hmac(byte[] key, string data)
    {
        return HMACSHA256.HashData(key, Encoding.UTF8.GetBytes(data));
    }

    private static string ToHex(byte[] bytes)
    {
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
