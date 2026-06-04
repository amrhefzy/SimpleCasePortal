using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SimpleCasePortal.Application.Interfaces;

namespace SimpleCasePortal.Infrastructure.Storage;

public static class FileStorageServiceFactory
{
    public static IFileStorageService Create(IServiceProvider serviceProvider)
    {
        var options = serviceProvider.GetRequiredService<IOptions<StorageOptions>>().Value;

        return string.Equals(options.Provider, "LocalDevelopment", StringComparison.OrdinalIgnoreCase)
            ? ActivatorUtilities.CreateInstance<LocalDevelopmentStorageService>(serviceProvider)
            : ActivatorUtilities.CreateInstance<DigitalOceanSpacesStorageService>(serviceProvider);
    }
}
