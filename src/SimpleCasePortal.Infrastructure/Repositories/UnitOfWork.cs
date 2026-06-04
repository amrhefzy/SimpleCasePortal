using SimpleCasePortal.Application.Interfaces;
using SimpleCasePortal.Infrastructure.Data;

namespace SimpleCasePortal.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _dbContext;
    private readonly Dictionary<Type, object> _repositories = [];

    public UnitOfWork(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IRepository<TEntity> Repository<TEntity>()
        where TEntity : class
    {
        var entityType = typeof(TEntity);

        if (!_repositories.TryGetValue(entityType, out var repository))
        {
            repository = new GenericRepository<TEntity>(_dbContext);
            _repositories.Add(entityType, repository);
        }

        return (IRepository<TEntity>)repository;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
