using System.Diagnostics;
using Orleans.Configuration;

await Host.CreateDefaultBuilder(args)
    .UseOrleans((context, silo) =>
    {
        if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
        {
            var redisConnectionString = context.Configuration["Redis:ConnectionString"] ?? "localhost";
            var orleansGatewayPort = int.Parse(context.Configuration["Orleans:GatewayPort"] ?? "30000");

            Debug.WriteLine($"Redis connection-string: {redisConnectionString}");
            Debug.WriteLine($"Orleans gateway port: {orleansGatewayPort}");

            silo
                .UseRedisClustering(redisConnectionString)
                .UseDashboard()
                .Configure<EndpointOptions>(options => options.GatewayPort = orleansGatewayPort)
                .ConfigureLogging(logger => logger.AddConsole());
        }
        else
        {
            silo
                .UseLocalhostClustering()
                .UseDashboard()
                .ConfigureLogging(logger => logger.AddConsole());
        }
    })
    .RunConsoleAsync();
