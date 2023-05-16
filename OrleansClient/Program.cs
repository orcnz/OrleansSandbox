using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrleansGrainInterfaces;

var host = Host.CreateDefaultBuilder(args)
    .UseOrleansClient(client =>
    {
        // This works (connects to cluster on port 30000)
        //client.UseStaticClustering(new IPEndPoint(IPAddress.Loopback, 30000));

        // This doesn't (fails to connect to cluster on ports 30001 and 30002)
        client.UseStaticClustering(
            new IPEndPoint(IPAddress.Loopback, 30000),
            new IPEndPoint(IPAddress.Loopback, 30001),
            new IPEndPoint(IPAddress.Loopback, 30002));
    })
    .ConfigureLogging(logging => logging.AddConsole())
    .Build();

await host.StartAsync();

var client = host.Services.GetRequiredService<IClusterClient>();

while (true)
{
    // Ask for input
    Console.Write("Enter greeting: ");
    var greeting = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(greeting)) break;

    // Send the input to the grain and get a response
    var helloGrain = client.GetGrain<IHello>(0);
    var response = await helloGrain.SayHello(greeting.Trim());
    Console.WriteLine($"Received \"{response}\" from silo.");
}

Console.WriteLine("Press any key to exit...");
Console.ReadKey(true);

await host.StopAsync();