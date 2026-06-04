namespace SimpleCasePortal.Application.DTOs.Files;

public sealed class SignedFileUrlDto
{
    public int FileId { get; set; }

    public string FileName { get; set; } = default!;

    public string Url { get; set; } = default!;

    public DateTime ExpiresOnUtc { get; set; }
}
