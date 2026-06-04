using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleCasePortal.Application.Interfaces;
using SimpleCasePortal.Infrastructure.Auth;
using SimpleCasePortal.Infrastructure.Cases;
using SimpleCasePortal.Infrastructure.Data;
using SimpleCasePortal.Infrastructure.ExternalApis;
using SimpleCasePortal.Infrastructure.Files;
using SimpleCasePortal.Infrastructure.Repositories;
using SimpleCasePortal.Infrastructure.Security;
using SimpleCasePortal.Infrastructure.Storage;

namespace SimpleCasePortal.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.Configure<StorageOptions>(configuration.GetSection("Storage"));
        services.Configure<ExternalApisOptions>(configuration.GetSection("ExternalApis"));

        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICaseNumberGenerator, CaseNumberGenerator>();
        services.AddScoped<ICaseFileService, CaseFileService>();
        services.AddScoped<ICaseService, CaseService>();
        services.AddScoped<IExternalSyncService, ExternalSyncService>();
        services.AddScoped<ICaseAuthorizationService, CaseAuthorizationService>();
        services.AddScoped(FileStorageServiceFactory.Create);
        services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddScoped<IPermissionService, PermissionService>();

        var useFakeExternalClients = environment.IsDevelopment() &&
            configuration.GetValue<bool>("ExternalApis:UseFakeClientsInDevelopment");

        if (useFakeExternalClients)
        {
            services.AddScoped<IExternalApiClient>(_ => new FakeExternalApiClient(Domain.Enums.SyncTargetEnum.DentistApp));
            services.AddScoped<IExternalApiClient>(_ => new FakeExternalApiClient(Domain.Enums.SyncTargetEnum.WorkflowApp));
            services.AddScoped<IExternalApiClient>(_ => new FakeExternalApiClient(Domain.Enums.SyncTargetEnum.ProductionApp));
        }
        else
        {
            services.AddScoped<IExternalApiClient, DentistApiClient>();
            services.AddScoped<IExternalApiClient, WorkflowApiClient>();
            services.AddScoped<IExternalApiClient, ProductionApiClient>();
        }

        return services;
    }
}
