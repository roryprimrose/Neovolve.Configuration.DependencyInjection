namespace TestHost;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder()
            .ConfigureWith<Config>()
            .ConfigureServices(
                services =>
                {
                    services.AddHostedService<RootConcreteService>();
                    services.AddHostedService<RootInterfaceService>();

                    services.AddHostedService<ConcreteService>();
                    services.AddHostedService<InterfaceService>();
                    services.AddHostedService<OptionsConcreteService>();
                    services.AddHostedService<OptionsInterfaceService>();
                    services.AddHostedService<SnapshotConcreteService>();
                    services.AddHostedService<SnapshotInterfaceService>();
                    services.AddHostedService<MonitorConcreteService>();
                    services.AddHostedService<MonitorInterfaceService>();
                });

        await builder.RunConsoleAsync().ConfigureAwait(false);

        Console.WriteLine("Completed");
    }
}