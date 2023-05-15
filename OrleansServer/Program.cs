using System.Net;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;

var builder = new HostBuilder()
    .UseOrleans(silo =>
    {
        if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
        {
            silo.UseLocalhostClustering()
                .Configure<EndpointOptions>(options =>
                {
                    options.GatewayListeningEndpoint = GetGatewayListeningEndpoint();
                })
                .ConfigureLogging(logger => logger.AddConsole());
        }
        else
        {
            silo.UseLocalhostClustering()
                .ConfigureLogging(logger => logger.AddConsole());
        }
    });

var host = builder.Build();

if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
{
    await host.RunAsync();
}
else
{
    await host.StartAsync();

    Console.WriteLine("Silo running, press any key to terminate...");
    Console.ReadKey(true);

    await host.StopAsync();
}

static IPEndPoint GetGatewayListeningEndpoint(int port = 30000)
{
    // Find the host IP address in the container, probably "172.72.0.2" for docker.
    var host = Dns.GetHostEntry(Dns.GetHostName());
    var address = host.AddressList.Single();
    return new IPEndPoint(address, port);
}