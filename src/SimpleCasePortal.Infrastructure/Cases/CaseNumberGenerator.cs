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

        var caseNumbers = await _dbContext.Cases
            .AsNoTracking()
            .Where(caseEntity => caseEntity.CaseNumber.StartsWith(prefix))
            .Select(caseEntity => caseEntity.CaseNumber)
            .ToArrayAsync(cancellationToken);

        var nextSequence = caseNumbers
            .Select(caseNumber => int.TryParse(caseNumber[prefix.Length..], out var sequence) ? sequence : 0)
            .DefaultIfEmpty(0)
            .Max() + 1;

        return $"{prefix}{nextSequence:000000}";
    }
}
