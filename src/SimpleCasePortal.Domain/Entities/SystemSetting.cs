namespace SimpleCasePortal.Domain.Entities;

public sealed class SystemSetting
{
    public int Id { get; set; }

    public string Key { get; set; } = default!;

    public string? Value { get; set; }

    public string? Description { get; set; }

    public bool IsEncrypted { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }
}
