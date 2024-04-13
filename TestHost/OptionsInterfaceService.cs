namespace TestHost;

using Microsoft.Extensions.Options;

internal class OptionsInterfaceService : ConsoleUpdateService
{
    private readonly IOptions<IThirdConfig> _config;

    public OptionsInterfaceService(IOptions<IThirdConfig> config)
    {
        _config = config;
    }

    protected override Task DoWork(CancellationToken cancellationToken)
    {
        return WriteValue(_config.Value.ThirdValue + " with timeout " + _config.Value.Timeout);
    }
}