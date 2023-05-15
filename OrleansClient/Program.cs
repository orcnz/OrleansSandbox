using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrleansGrainInterfaces;

var builder = new HostBuilder()
    .UseOrleansClient(client =>
    {
        client.UseLocalhostClustering();
    })
    .ConfigureLogging(logging => logging.AddConsole());

var host = builder.Build();

await host.StartAsync();

var client = host.Services.GetRequiredService<IClusterClient>();

while (true)
{
    Console.Write("Enter greeting: ");
    var greeting = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(greeting)) break;

    var helloGrain = client.GetGrain<IHello>(0);
    var response = await helloGrain.SayHello(greeting.Trim());
    Console.WriteLine($"Received \"{response}\" from silo.");
}

Console.WriteLine("Press any key to exit...");
Console.ReadKey(true);

await host.StopAsync();