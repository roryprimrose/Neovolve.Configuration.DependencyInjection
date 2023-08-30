namespace TestHost
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Neovolve.Configuration.DependencyInjection;

    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var builder = Host.CreateDefaultBuilder()
                .ConfigureWith<Config>()
                .ConfigureLogging(
                    builder => { builder.AddConsole(); })
                .ConfigureServices(
                    services =>
                    {
                        services.AddHostedService<RootConcreteService>();
                        services.AddHostedService<RootInterfaceService>();
                        services.AddHostedService<MonitorConcreteService>();
                        services.AddHostedService<MonitorInterfaceService>();
                        services.AddHostedService<SnapshotConcreteService>();
                        services.AddHostedService<SnapshotInterfaceService>();
                        services.AddHostedService<ConcreteService>();
                        services.AddHostedService<InterfaceService>();
                    });

            await builder.RunConsoleAsync().ConfigureAwait(false);

            Console.WriteLine("Completed");
        }
    }
}