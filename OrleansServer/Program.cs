await Host.CreateDefaultBuilder(args)
    .UseOrleans((context, silo) =>
    {
        silo.UseRedisClustering(context.Configuration["Redis:ConnectionString"] ?? "localhost")
            .UseDashboard()
            .ConfigureLogging(logger => logger.AddConsole());
    })
    .RunConsoleAsync();
