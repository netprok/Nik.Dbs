namespace Nik.Common;

public static class ServicesExtensions
{
    public static IServiceCollection AddNikCommon(this IServiceCollection services)
    {
        services.AddSingleton<IDbScaffolder, DbScaffolder>();

        return services;
    }
}