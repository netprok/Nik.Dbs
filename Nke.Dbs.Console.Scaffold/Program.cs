var builder = Host.CreateApplicationBuilder(args);
builder.Services
    .InitContext(@"_assets/scaffold_definitions.json", @"_assets/insert_statement_definitions.json")
    .AddNikFiles()
    .AddNikCommon()
    .AddNikDbs()
    .AddSingleton<IScaffoldPropertyNameGenerater, JtlScaffoldPropertyNameGenerater>();

using IHost host = builder.Build();

await host.Services.GetService<IDbScaffolder>()!.ScaffoldAsync(Context.Configuration.GetSection<ScaffoldDefinition>()!);
//await host.Services.GetService<IInsertStatementGenerator>()!.CreateAsync(Context.Configuration.GetSection<InsertStatementDefinitions>()!);

await host.RunAsync();