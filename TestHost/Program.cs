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

                    //services.AddHostedService<MonitorConcreteService>();
                    //services.AddHostedService<MonitorInterfaceService>();
                    //services.AddHostedService<SnapshotConcreteService>();
                    //services.AddHostedService<SnapshotInterfaceService>();
                    services.AddHostedService<ConcreteService>();
                    services.AddHostedService<InterfaceService>();
                });

        await builder.RunConsoleAsync().ConfigureAwait(false);

        Console.WriteLine("Completed");
    }
}