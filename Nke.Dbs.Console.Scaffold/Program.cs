var builder = Host.CreateApplicationBuilder(args);
builder.Services
    .InitContext(@"_assets/definitions.json")
    .AddNikFiles()
    .AddNikDbs()
    .AddSingleton<IScaffoldPropertyNameGenerater, JtlScaffoldPropertyNameGenerater>();

using IHost host = builder.Build();

var scaffolder = host.Services.GetService<IDbScaffolder>()!;
await scaffolder.ScaffoldAsync(Context.Configuration.GetSection<ScaffoldDefinition>()!);

await host.RunAsync();