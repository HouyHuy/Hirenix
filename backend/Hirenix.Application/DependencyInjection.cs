using Hirenix.Application.Interfaces;
using Hirenix.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Hirenix.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}
