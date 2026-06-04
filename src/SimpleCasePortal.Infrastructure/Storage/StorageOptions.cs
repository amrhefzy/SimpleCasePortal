namespace SimpleCasePortal.Infrastructure.Storage;

public sealed class StorageOptions
{
    public string Provider { get; set; } = "DigitalOceanSpaces";

    public string? ServiceUrl { get; set; }

    public string? Region { get; set; }

    public string? BucketName { get; set; }

    public string? AccessKey { get; set; }

    public string? SecretKey { get; set; }

    public int SignedUrlExpiryMinutes { get; set; } = 15;

    public int MaxFileSizeMb { get; set; } = 200;

    public string? LocalRootPath { get; set; }

    public string? LocalBaseUrl { get; set; }
}
