namespace Nik.Dbs;

public static class ServicesExtensions
{
    public static IServiceCollection AddNikDbs(this IServiceCollection services)
    {
        services.AddSingleton<IDbScaffolder, DbScaffolder>();
        services.AddSingleton<IInsertStatementGenerator, InsertStatementGenerator>();
        services.AddSingleton<IFieldGenerater, FieldGenerater>();
        services.AddSingleton<ISchemaTableGenerater, SchemaTableGenerater>();

        return services;
    }
}