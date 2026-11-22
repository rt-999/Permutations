using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Permutations.Application.Abstractions;
using Permutations.Infrastructure.Sessions;

namespace Permutations.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInMemorySessionStore(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<SessionStoreOptions>(config.GetSection(SessionStoreOptions.Section));
        services.AddSingleton<InMemorySessionStore>();
        services.AddSingleton<ISessionStore>(sp => sp.GetRequiredService<InMemorySessionStore>());
        services.AddHostedService<SessionCleanupService>();
        return services;
    }

    // Future: public static IServiceCollection AddRedisSessionStore(...)
}
