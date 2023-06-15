
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Get all types from the assembly
        var assembly = Assembly.GetExecutingAssembly();
        var types = assembly.GetTypes();

        // Find all cache services and register them
        var cacheServiceTypes = types.Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("CacheService"));
        foreach (var cacheServiceType in cacheServiceTypes)
        {
            var implementedInterface = cacheServiceType.GetInterfaces().FirstOrDefault();
            if (implementedInterface != null)
            {
                services.AddScoped(implementedInterface, cacheServiceType);
            }
        }

        // Find all repository interfaces and their implementations, including cached versions
        var repositoryInterfaceTypes = types.Where(t => t.IsInterface && t.Name.EndsWith("Repository"));
        foreach (var repositoryInterfaceType in repositoryInterfaceTypes)
        {
            // Find the corresponding repository implementation
            var repositoryImplementationType = types.FirstOrDefault(t => t.IsClass && !t.IsAbstract && t.Name == repositoryInterfaceType.Name.Replace("I", ""));
            if (repositoryImplementationType != null)
            {
                services.AddScoped(repositoryInterfaceType, repositoryImplementationType);

                // Check if there is a cached version of the repository
                var cachedRepositoryType = types.FirstOrDefault(t => t.IsClass && !t.IsAbstract && t.Name == "Cached" + repositoryInterfaceType.Name.Replace("I", ""));
                if (cachedRepositoryType != null)
                {
                    services.AddScoped(repositoryInterfaceType, serviceProvider =>
                    {
                        // Resolve the repository implementation
                        var repositoryImplementation = serviceProvider.GetRequiredService(repositoryImplementationType);

                        // Resolve the cache service
                        var cacheServiceInterface = repositoryInterfaceType.GetInterfaces().FirstOrDefault();
                        var cacheService = serviceProvider.GetRequiredService(cacheServiceInterface);

                        // Create an instance of the cached repository using reflection
                        var cachedRepositoryInstance = Activator.CreateInstance(cachedRepositoryType, repositoryImplementation, cacheService);

                        return cachedRepositoryInstance;
                    });
                }
            }
        }

        // Add any additional services or configurations
        // ...

        // Build the service provider
        var serviceProvider = services.BuildServiceProvider();

        // Resolve any required services
        // ...
    }
}
