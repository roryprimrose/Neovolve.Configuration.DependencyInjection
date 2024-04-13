namespace TestHost;

using Microsoft.Extensions.Options;

internal class MonitorConcreteService : ConsoleUpdateService
{
    private readonly IOptionsMonitor<ThirdConfig> _config;

    public MonitorConcreteService(IOptionsMonitor<ThirdConfig> config)
    {
        _config = config;

        //_config.OnChange(x =>
        //{
        //    Console.WriteLine("Third value changed from IOptionsMonitor<ThirdConfig>");
        //});
    }

    protected override Task DoWork(CancellationToken cancellationToken)
    {
        return WriteValue(_config.CurrentValue.ThirdValue + " with timeout " + _config.CurrentValue.Timeout);
    }
}