namespace SimpleCasePortal.Application.Interfaces;

public interface ICaseAuthorizationService
{
    Task<bool> CanAccessCaseAsync(string userId, int caseId, CancellationToken cancellationToken = default);
}
