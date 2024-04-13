namespace TestHost;

using Microsoft.Extensions.Options;

internal class MonitorInterfaceService : ConsoleUpdateService
{
    private readonly IOptionsMonitor<IThirdConfig> _config;

    public MonitorInterfaceService(IOptionsMonitor<IThirdConfig> config)
    {
        _config = config;

        //_config.OnChange(x =>
        //{
        //    Console.WriteLine("Third value changed from IOptionsMonitor<IThirdConfig>");
        //});
    }

    protected override Task DoWork(CancellationToken cancellationToken)
    {
        return WriteValue(_config.CurrentValue.ThirdValue + " with timeout " + _config.CurrentValue.Timeout);
    }
}