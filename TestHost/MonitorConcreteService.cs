namespace TestHost
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    internal class MonitorConcreteService : TimerHostedService
    {
        private readonly IOptionsMonitor<FirstConfig> _config;
        private readonly ILogger<MonitorConcreteService> _logger;

        public MonitorConcreteService(IOptionsMonitor<FirstConfig> config, ILogger<MonitorConcreteService> logger)
        {
            _config = config;
            _logger = logger;
        }

        protected override Task DoWork(CancellationToken cancellationToken)
        {
            _logger.LogInformation(_config.CurrentValue.FirstValue);

            return Task.CompletedTask;
        }
    }
}