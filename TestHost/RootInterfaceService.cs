namespace TestHost;

using Microsoft.Extensions.Logging;

internal class RootInterfaceService : TimerHostedService
{
    private readonly Config _config;
    private readonly ILogger<RootInterfaceService> _logger;

    public RootInterfaceService(Config config, ILogger<RootInterfaceService> logger)
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