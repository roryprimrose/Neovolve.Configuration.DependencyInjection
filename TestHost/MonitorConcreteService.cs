namespace TestHost
{
    using Microsoft.Extensions.Options;

    internal class MonitorConcreteService : ConsoleUpdateService
    {
        private readonly IOptionsMonitor<FirstConfig> _config;

        public MonitorConcreteService(IOptionsMonitor<FirstConfig> config)
        {
            _config = config;

            //_config.OnChange(x =>
            //{
            //    Console.WriteLine("First value changed from IOptionsMonitor<FirstConfig>");
            //});
        }

        protected override Task DoWork(CancellationToken cancellationToken)
        {
            return WriteValue(_config.CurrentValue.FirstValue);
        }
    }
}