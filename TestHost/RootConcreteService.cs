namespace TestHost;

using Microsoft.Extensions.Logging;

internal class RootConcreteService : TimerHostedService
{
    private readonly Config _config;
    private readonly ILogger<RootConcreteService> _logger;

    public RootConcreteService(Config config, ILogger<RootConcreteService> logger)
    {
        _config = config;
        _logger = logger;
    }

    protected override Task DoWork(CancellationToken cancellationToken)
    {
        //Console.WriteLine("Root: " + _config.CurrentValue.RootValue);
        _logger.LogInformation(_config.RootValue);

        return Task.CompletedTask;
    }
}