using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrleansGrainInterfaces;

var builder = Host.CreateDefaultBuilder(args)
    .UseOrleansClient(client =>
    {
        client.UseStaticClustering(new IPEndPoint(IPAddress.Loopback, 30000));

        //client.UseStaticClustering(
        //    new IPEndPoint(IPAddress.Loopback, 30000),
        //    new IPEndPoint(IPAddress.Loopback, 30001),
        //    new IPEndPoint(IPAddress.Loopback, 30002));
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