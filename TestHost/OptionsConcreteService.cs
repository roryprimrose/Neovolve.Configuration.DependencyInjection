namespace TestHost;

using Microsoft.Extensions.Options;

internal class OptionsConcreteService : ConsoleUpdateService
{
    private readonly IOptions<ThirdConfig> _config;

    public OptionsConcreteService(IOptions<ThirdConfig> config)
    {
        _config = config;
    }

    protected override Task DoWork(CancellationToken cancellationToken)
    {
        return WriteValue(_config.Value.ThirdValue + " with timeout " + _config.Value.Timeout);
    }
}