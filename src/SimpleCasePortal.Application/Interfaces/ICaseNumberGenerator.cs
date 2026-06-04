namespace SimpleCasePortal.Application.Interfaces;

public interface ICaseNumberGenerator
{
    Task<string> GenerateAsync(CancellationToken cancellationToken = default);
}
