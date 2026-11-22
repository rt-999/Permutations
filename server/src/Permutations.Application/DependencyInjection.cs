using Microsoft.Extensions.DependencyInjection;
using Permutations.Application.Services;
using Permutations.Domain.Algorithms;

namespace Permutations.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<INextPermutation, LexicographicNextPermutation>();
        services.AddSingleton<IPermutationUnranker, FactorialUnranker>();
        services.AddScoped<IPermutationService, PermutationService>();
        return services;
    }
}
