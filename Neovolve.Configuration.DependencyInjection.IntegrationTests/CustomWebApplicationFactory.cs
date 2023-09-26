namespace Neovolve.Configuration.DependencyInjection.IntegrationTests;

using Microsoft.AspNetCore.Mvc.Testing;

public class CustomWebApplicationFactory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        var environment = host.Services.GetRequiredService<IWebHostEnvironment>();

        ConfigPath = Path.Combine(environment.ContentRootPath, "appsettings.json");

        return host;
    }

    public string ConfigPath { get; set; }
}