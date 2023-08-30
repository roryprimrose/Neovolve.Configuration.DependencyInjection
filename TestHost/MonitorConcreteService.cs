namespace TestHost
{
    using Microsoft.Extensions.Options;

    internal class MonitorConcreteService : ConsoleUpdateService
    {
        private readonly IOptionsMonitor<FirstConfig> _config;

        public MonitorConcreteService(IOptionsMonitor<FirstConfig> config)
        {
            _config = config;
        }

        protected override Task DoWork(CancellationToken cancellationToken)
        {
            return WriteValue(_config.CurrentValue.FirstValue);
        }
    }
}