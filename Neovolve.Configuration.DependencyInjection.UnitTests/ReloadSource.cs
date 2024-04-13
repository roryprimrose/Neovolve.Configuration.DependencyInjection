namespace Neovolve.Configuration.DependencyInjection.UnitTests;

using Microsoft.Extensions.Configuration;

internal class ReloadSource : IConfigurationSource
{
    private class ReloadProvider : ConfigurationProvider
    {
        public ReloadProvider(IDictionary<string, string> data)
        {
            foreach (var item in data)
            {
                Data[item.Key] = item.Value;
            }
        }

        public void Update(IDictionary<string, string> data)
        {
            foreach (var item in data)
            {
                Data[item.Key] = item.Value;
            }

            OnReload();
        }
    }

    private readonly ReloadProvider _provider;

    public ReloadSource(IDictionary<string, string> data)
    {
        _provider = new ReloadProvider(data);
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return _provider;
    }

    public void Update(IDictionary<string, string> data)
    {
        _provider.Update(data);
    }
}