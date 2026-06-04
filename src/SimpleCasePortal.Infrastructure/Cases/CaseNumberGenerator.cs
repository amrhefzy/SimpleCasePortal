using Microsoft.EntityFrameworkCore;
using SimpleCasePortal.Application.Interfaces;
using SimpleCasePortal.Infrastructure.Data;

namespace SimpleCasePortal.Infrastructure.Cases;

public sealed class CaseNumberGenerator : ICaseNumberGenerator
{
    private readonly AppDbContext _dbContext;

    public CaseNumberGenerator(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string> GenerateAsync(CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"CP-{year}-";

        var latestCaseNumber = await _dbContext.Cases
            .AsNoTracking()
            .Where(caseEntity => caseEntity.CaseNumber.StartsWith(prefix))
            .OrderByDescending(caseEntity => caseEntity.CaseNumber)
            .Select(caseEntity => caseEntity.CaseNumber)
            .FirstOrDefaultAsync(cancellationToken);

        var nextSequence = 1;
        if (!string.IsNullOrWhiteSpace(latestCaseNumber) &&
            int.TryParse(latestCaseNumber[prefix.Length..], out var latestSequence))
        {
            nextSequence = latestSequence + 1;
        }

        return $"{prefix}{nextSequence:000000}";
    }
}
