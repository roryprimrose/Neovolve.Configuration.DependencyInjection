namespace Neovolve.Configuration.DependencyInjection.IntegrationTests;

using FluentAssertions;
using WebTestHost;
using Xunit;
using Xunit.Abstractions;

public class InjectionTests
    : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _output;

    public InjectionTests(CustomWebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

    [Theory]
    [InlineData("/ConfigConcrete")]
    [InlineData("/ConfigOptionsConcrete")]
    [InlineData("/ConfigSnapshotConcrete")]
    [InlineData("/ConfigMonitorConcrete")]
    [InlineData("/ConfigInterface")]
    [InlineData("/ConfigOptionsInterface")]
    [InlineData("/ConfigSnapshotInterface")]
    [InlineData("/ConfigMonitorInterface")]
    [InlineData("/FirstConcrete")]
    [InlineData("/FirstOptionsConcrete")]
    [InlineData("/FirstSnapshotConcrete")]
    [InlineData("/FirstMonitorConcrete")]
    [InlineData("/FirstInterface")]
    [InlineData("/FirstOptionsInterface")]
    [InlineData("/FirstSnapshotInterface")]
    [InlineData("/FirstMonitorInterface")]
    [InlineData("/SecondConcrete")]
    [InlineData("/SecondOptionsConcrete")]
    [InlineData("/SecondSnapshotConcrete")]
    [InlineData("/SecondMonitorConcrete")]
    [InlineData("/SecondInterface")]
    [InlineData("/SecondOptionsInterface")]
    [InlineData("/SecondSnapshotInterface")]
    [InlineData("/SecondMonitorInterface")]
    [InlineData("/ThirdConcrete")]
    [InlineData("/ThirdOptionsConcrete")]
    [InlineData("/ThirdSnapshotConcrete")]
    [InlineData("/ThirdMonitorConcrete")]
    [InlineData("/ThirdInterface")]
    [InlineData("/ThirdOptionsInterface")]
    [InlineData("/ThirdSnapshotInterface")]
    [InlineData("/ThirdMonitorInterface")]
    public async Task GetInjectsExpectedTypes(string url)
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync(url);

        var content = await response.Content.ReadAsStringAsync();

        _output.WriteLine(content);

        response.EnsureSuccessStatusCode(); // Status Code 200-299

        response.Content.Headers.ContentType.ToString().Should().Be("application/json; charset=utf-8");
        content.Should().NotBeNullOrWhiteSpace();
    }
}