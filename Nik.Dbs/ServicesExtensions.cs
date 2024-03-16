using Nik.Dbs.Abstractions;

namespace Nik.Dbs;

public static class ServicesExtensions
{
    public static IServiceCollection AddNikDbs(this IServiceCollection services)
    {
        services.AddSingleton<IDbScaffolder, DbScaffolder>();

        return services;
    }
}