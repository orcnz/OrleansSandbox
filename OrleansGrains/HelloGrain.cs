using Microsoft.Extensions.Logging;
using OrleansGrainInterfaces;

namespace OrleansGrains;

public class HelloGrain : Grain, IHello
{
    private readonly ILogger<HelloGrain> _logger;

    public HelloGrain(ILogger<HelloGrain> logger)
    {
        _logger = logger;
    }

    public ValueTask<string> SayHello(string greeting)
    {
        _logger.LogInformation($"Client sent {greeting}");

        return ValueTask.FromResult($"Hello, {greeting} (from {this.GetGrainId()})");
    }
}