namespace TestHost;

using Microsoft.Extensions.Logging;

internal class ConcreteService : TimerHostedService
{
    private readonly SecondConfig _config;
    private readonly ILogger<ConcreteService> _logger;

    public ConcreteService(SecondConfig config, ILogger<ConcreteService> logger)
    {
        _config = config;
        _logger = logger;
    }

    protected override Task DoWork(CancellationToken cancellationToken)
    {
        _logger.LogInformation(_config.SecondValue);

        return Task.CompletedTask;
    }
}