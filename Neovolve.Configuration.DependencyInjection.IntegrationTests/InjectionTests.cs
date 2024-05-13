namespace Neovolve.Configuration.DependencyInjection.IntegrationTests;

using System.Diagnostics;
using FluentAssertions;
using ModelBuilder;
using Newtonsoft.Json;
using WebTestHost;
using Xunit;
using Xunit.Abstractions;

public sealed class InjectionTests
    : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _output;
    private RootConfig? _originalConfig;

    private string? _originalData;

    public InjectionTests(CustomWebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

    [Theory]
    [InlineData("/ConfigConcrete")]
    [InlineData("/ConfigInterface")]
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
        using var client = _factory.CreateClient();

        var response = await client.GetAsync(url);

        var content = await response.Content.ReadAsStringAsync();

        _output.WriteLine(content);

        response.EnsureSuccessStatusCode(); // Status Code 200-299

        response.Content.Headers.ContentType.Should().NotBeNull();
        response.Content.Headers.ContentType!.ToString().Should().Be("application/json; charset=utf-8");
        content.Should().NotBeNullOrWhiteSpace();
    }

    [Theory]
    [InlineData("/ConfigConcrete")]
    [InlineData("/ConfigInterface")]
    public async Task GetReturnsCurrentConfig(string url)
    {
        using var client = _factory.CreateClient();

        var expected = await GetConfig();

        var actual = await GetData<RootConfig>(client, url);

        actual.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData("/FirstConcrete")]
    [InlineData("/FirstOptionsConcrete")]
    [InlineData("/FirstSnapshotConcrete")]
    [InlineData("/FirstMonitorConcrete")]
    [InlineData("/FirstInterface")]
    [InlineData("/FirstOptionsInterface")]
    [InlineData("/FirstSnapshotInterface")]
    [InlineData("/FirstMonitorInterface")]
    public async Task GetReturnsCurrentFirst(string url)
    {
        using var client = _factory.CreateClient();

        var expected = await GetConfig();

        var actual = await GetData<FirstConfig>(client, url);

        actual.Should().BeEquivalentTo(expected.First);
    }

    [Theory]
    [InlineData("/SecondConcrete")]
    [InlineData("/SecondOptionsConcrete")]
    [InlineData("/SecondSnapshotConcrete")]
    [InlineData("/SecondMonitorConcrete")]
    [InlineData("/SecondInterface")]
    [InlineData("/SecondOptionsInterface")]
    [InlineData("/SecondSnapshotInterface")]
    [InlineData("/SecondMonitorInterface")]
    public async Task GetReturnsCurrentSecond(string url)
    {
        using var client = _factory.CreateClient();

        var expected = await GetConfig();

        var actual = await GetData<SecondConfig>(client, url);

        actual.Should().BeEquivalentTo(expected.First.Second);
    }

    [Theory]
    [InlineData("/ThirdConcrete")]
    [InlineData("/ThirdOptionsConcrete")]
    [InlineData("/ThirdSnapshotConcrete")]
    [InlineData("/ThirdMonitorConcrete")]
    [InlineData("/ThirdInterface")]
    [InlineData("/ThirdOptionsInterface")]
    [InlineData("/ThirdSnapshotInterface")]
    [InlineData("/ThirdMonitorInterface")]
    public async Task GetReturnsCurrentThird(string url)
    {
        using var client = _factory.CreateClient();

        var expected = await GetConfig();

        var actual = await GetData<ThirdConfig>(client, url);

        actual.Should().BeEquivalentTo(expected.First.Second.Third);
    }

    [Theory]
    [InlineData("/FirstOptionsConcrete")]
    [InlineData("/FirstOptionsInterface")]
    public async Task GetReturnsOriginalFirstDataWhenDataUpdatedForOptionsUsage(string url)
    {
        using var client = _factory.CreateClient();

        try
        {
            var firstExpected = await GetConfig();

            var firstActual = await GetData<FirstConfig>(client, url);

            firstActual.Should().BeEquivalentTo(firstExpected.First);

            var secondExpected = await UpdateConfig().ConfigureAwait(false);

            await Task.Delay(2000);

            var secondActual = await GetData<FirstConfig>(client, url);

            firstActual.Should().BeEquivalentTo(secondActual);
            secondActual.Should().NotBeEquivalentTo(secondExpected.First);
        }
        finally
        {
            await RestoreConfig(client);
        }
    }

    [Theory]
    [InlineData("/ConfigConcrete")]
    [InlineData("/ConfigInterface")]
    public async Task GetReturnsOriginalRootConfigWhenDataChanged(string url)
    {
        using var client = _factory.CreateClient();

        try
        {
            var firstExpected = await GetConfig();

            await UpdateConfig();

            var secondActual = await GetData<RootConfig>(client, url);

            secondActual.Should().BeEquivalentTo(firstExpected);
        }
        finally
        {
            await RestoreConfig(client);
        }
    }

    [Theory]
    [InlineData("/SecondOptionsConcrete")]
    [InlineData("/SecondOptionsInterface")]
    public async Task GetReturnsOriginalSecondDataWhenDataUpdatedForOptionsUsage(string url)
    {
        using var client = _factory.CreateClient();

        try
        {
            var firstExpected = await GetConfig();

            var firstActual = await GetData<SecondConfig>(client, url);

            firstActual.Should().BeEquivalentTo(firstExpected.First.Second);

            var secondExpected = await UpdateConfig().ConfigureAwait(false);

            await Task.Delay(2000);

            var secondActual = await GetData<SecondConfig>(client, url);

            firstActual.Should().BeEquivalentTo(secondActual);
            secondActual.Should().NotBeEquivalentTo(secondExpected.First.Second);
        }
        finally
        {
            await RestoreConfig(client);
        }
    }

    [Theory]
    [InlineData("/ThirdOptionsConcrete")]
    [InlineData("/ThirdOptionsInterface")]
    public async Task GetReturnsOriginalThirdDataWhenDataUpdatedForOptionsUsage(string url)
    {
        using var client = _factory.CreateClient();

        try
        {
            var firstExpected = await GetConfig();

            var firstActual = await GetData<ThirdConfig>(client, url);

            firstActual.Should().BeEquivalentTo(firstExpected.First.Second.Third);

            var secondExpected = await UpdateConfig().ConfigureAwait(false);

            await Task.Delay(2000);

            var secondActual = await GetData<ThirdConfig>(client, url);

            firstActual.Should().BeEquivalentTo(secondActual);
            secondActual.Should().NotBeEquivalentTo(secondExpected.First.Second.Third);
        }
        finally
        {
            await RestoreConfig(client);
        }
    }

    [Theory]
    [InlineData("/FirstConcrete")]
    [InlineData("/FirstSnapshotConcrete")]
    [InlineData("/FirstMonitorConcrete")]
    [InlineData("/FirstInterface")]
    [InlineData("/FirstSnapshotInterface")]
    [InlineData("/FirstMonitorInterface")]
    public async Task GetReturnsUpdatedFirst(string url)
    {
        using var client = _factory.CreateClient();

        try
        {
            var firstExpected = await GetConfig();

            var firstActual = await GetData<FirstConfig>(client, url);

            firstActual.Should().BeEquivalentTo(firstExpected.First);

            var secondExpected = await UpdateConfig().ConfigureAwait(false);

            var secondActual =
                await WaitForUpdatedData<FirstConfig>(client, url,
                    x => x.FirstValue == secondExpected.First.FirstValue);

            firstActual.Should().NotBeEquivalentTo(secondActual);
            secondActual.Should().BeEquivalentTo(secondExpected.First);
        }
        finally
        {
            await RestoreConfig(client);
        }
    }

    [Theory]
    [InlineData("/SecondConcrete")]
    [InlineData("/SecondSnapshotConcrete")]
    [InlineData("/SecondMonitorConcrete")]
    [InlineData("/SecondInterface")]
    [InlineData("/SecondSnapshotInterface")]
    [InlineData("/SecondMonitorInterface")]
    public async Task GetReturnsUpdatedSecond(string url)
    {
        using var client = _factory.CreateClient();

        try
        {
            var firstExpected = await GetConfig();

            var firstActual = await GetData<SecondConfig>(client, url);

            firstActual.Should().BeEquivalentTo(firstExpected.First.Second);

            var secondExpected = await UpdateConfig().ConfigureAwait(false);

            var secondActual =
                await WaitForUpdatedData<SecondConfig>(client, url,
                    x => x.SecondValue == secondExpected.First.Second.SecondValue);

            firstActual.Should().NotBeEquivalentTo(secondActual);
            secondActual.Should().BeEquivalentTo(secondExpected.First.Second);
        }
        finally
        {
            await RestoreConfig(client);
        }
    }

    [Theory]
    [InlineData("/ThirdConcrete")]
    [InlineData("/ThirdSnapshotConcrete")]
    [InlineData("/ThirdMonitorConcrete")]
    [InlineData("/ThirdInterface")]
    [InlineData("/ThirdSnapshotInterface")]
    [InlineData("/ThirdMonitorInterface")]
    public async Task GetReturnsUpdatedThird(string url)
    {
        using var client = _factory.CreateClient();

        try
        {
            var firstExpected = await GetConfig();

            var firstActual = await GetData<ThirdConfig>(client, url);

            firstActual.Should().BeEquivalentTo(firstExpected.First.Second.Third);

            var secondExpected = await UpdateConfig().ConfigureAwait(false);

            var secondActual =
                await WaitForUpdatedData<ThirdConfig>(client, url,
                    x => x.ThirdValue == secondExpected.First.Second.Third.ThirdValue);

            firstActual.Should().NotBeEquivalentTo(secondActual);
            secondActual.Should().BeEquivalentTo(secondExpected.First.Second.Third);
        }
        finally
        {
            await RestoreConfig(client);
        }
    }

    private async Task<RootConfig> GetConfig()
    {
        var data = await File.ReadAllTextAsync(_factory.ConfigPath).ConfigureAwait(false);

        _output.WriteLine("Disk config: " + data);

        var config = JsonConvert.DeserializeObject<RootConfig>(data)!;

        if (_originalData == null)
        {
            _originalData = data;
            _originalConfig = config;
        }

        return config;
    }

    private async Task<T> GetData<T>(HttpClient client, string url)
    {
        var response = await client.GetAsync(url).ConfigureAwait(false);

        response.EnsureSuccessStatusCode(); // Status Code 200-299

        var data = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        _output.WriteLine(url + ": " + data);

        var actual = JsonConvert.DeserializeObject<T>(data);

        actual.Should().NotBeNull();

        return actual!;
    }

    private async Task RestoreConfig(HttpClient client)
    {
        if (string.IsNullOrWhiteSpace(_originalData) == false
            && _originalConfig != null)
        {
            // Restore the original config
            await File.WriteAllTextAsync(_factory.ConfigPath, _originalData).ConfigureAwait(false);

            // Wait until the data change is available via the API
            await WaitForUpdatedData<FirstConfig>(client, "/FirstMonitorConcrete",
                x => x.FirstValue == _originalConfig.First.FirstValue).ConfigureAwait(false);
        }
    }

    private async Task<RootConfig> UpdateConfig()
    {
        var newConfig = Model.Create<RootConfig>();

        var data = JsonConvert.SerializeObject(newConfig);

        _output.WriteLine("Updated disk config: " + data);

        await File.WriteAllTextAsync(_factory.ConfigPath, data).ConfigureAwait(false);

        await Task.Delay(1000).ConfigureAwait(false);

        return newConfig;
    }

    private async Task<T> WaitForUpdatedData<T>(HttpClient client, string url, Predicate<T> predicate)
    {
        var timestamp = Stopwatch.StartNew();
        var maxTime = TimeSpan.FromSeconds(5);
        T data;

        do
        {
            if (timestamp.Elapsed > maxTime)
            {
                throw new TimeoutException();
            }

            _output.WriteLine("Waiting for data change " + timestamp.ElapsedMilliseconds);

            await Task.Delay(100).ConfigureAwait(false);

            data = await GetData<T>(client, url).ConfigureAwait(false);
        } while (predicate(data) == false);

        return data;
    }
}