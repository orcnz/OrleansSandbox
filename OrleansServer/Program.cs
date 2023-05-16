using Orleans.Configuration;

await Host.CreateDefaultBuilder(args)
    .UseOrleans((context, silo) =>
    {
        silo.UseRedisClustering(context.Configuration["Redis:ConnectionString"] ?? "localhost")
            .UseDashboard()
            .Configure<EndpointOptions>(options =>
            {
                options.GatewayPort = int.Parse(context.Configuration["Orleans:GatewayPort"] ?? "30000");
            })
            .ConfigureLogging(logger => logger.AddConsole());
    })
    .RunConsoleAsync();
