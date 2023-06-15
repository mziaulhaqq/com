
using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Register using reflection
        RegisterCachedRepositories(services);

        // Other service registrations...

        // Configure other services, middleware, etc.
    }

    private void RegisterCachedRepositories(IServiceCollection services)
    {
        // Get all types in the current assembly
        var assembly = Assembly.GetExecutingAssembly();
        var types = assembly.GetTypes();

        // Get all types ending with "Repository" and starting with "Cached"
        var cachedRepositoryTypes = types.Where(t =>
            t.Name.EndsWith("Repository") &&
            t.Name.StartsWith("Cached")
        );

        foreach (var cachedRepoType in cachedRepositoryTypes)
        {
            // Get the corresponding interface
            var interfaceType = cachedRepoType.GetInterfaces()
                .FirstOrDefault(i => i.Name == "I" + cachedRepoType.Name);

            if (interfaceType != null)
            {
                // Register the interface with the cached repository implementation
                services.AddScoped(interfaceType, provider =>
                {
                    // Get the decorator repository and cache service
                    var decoratorType = types.FirstOrDefault(t =>
                        t.Name == cachedRepoType.Name.Substring(6) // Remove "Cached" prefix
                    );

                    var cacheServiceType = types.FirstOrDefault(t =>
                        t.Name == "I" + interfaceType.Name.Replace("Repository", "CacheService")
                    );

                    if (decoratorType != null && cacheServiceType != null)
                    {
                        var decorator = provider.GetRequiredService(decoratorType);
                        var cacheService = provider.GetRequiredService(cacheServiceType);

                        // Create an instance of the cached repository
                        var instance = Activator.CreateInstance(cachedRepoType, decorator, cacheService);
                        return instance;
                    }

                    return null;
                });
            }
        }
    }
}
