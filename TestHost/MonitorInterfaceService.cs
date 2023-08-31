namespace TestHost;

using Microsoft.Extensions.Options;

internal class MonitorInterfaceService : ConsoleUpdateService
{
    private readonly IOptionsMonitor<IFirstConfig> _config;

    public MonitorInterfaceService(IOptionsMonitor<IFirstConfig> config)
    {
        _config = config;

        //_config.OnChange(x =>
        //{
        //    Console.WriteLine("First value changed from IOptionsMonitor<IFirstConfig>");
        //});
    }

    protected override Task DoWork(CancellationToken cancellationToken)
    {
        return WriteValue(_config.CurrentValue.FirstValue);
    }
}