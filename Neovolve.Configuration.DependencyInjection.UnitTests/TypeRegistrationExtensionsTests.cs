namespace Neovolve.Configuration.DependencyInjection.UnitTests
{
    using Divergic.Logging.Xunit;
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using NSubstitute;
    using Xunit.Abstractions;

    public class TypeRegistrationExtensionsTests
    {
        private readonly ITestOutputHelper _output;

        public TypeRegistrationExtensionsTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void ConfigureWithDoesNotUpdateClassWithUpdatedDataWhenReloadDisabled()
        {
            var originalData = new Dictionary<string, string>
            {
                ["RootValue"] = "This is the root value",
                ["First:FirstValue"] = "This is the first value",
                ["First:Id"] = Guid.NewGuid().ToString(),
                ["First:Second:SecondValue"] = "This is the second value",
                ["First:Second:Third:ThirdValue"] = "This is the third value"
            };
            var updatedData = originalData.ToDictionary(x => x.Key, x => Guid.NewGuid().ToString());
            var source = new ReloadSource(originalData);

            var builder = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((_, configuration) => { configuration.Add(source); })
                .ConfigureWith<Config>(false);

            using var host = builder.Build();

            using var scope = host.Services.CreateScope();

            var actual = scope.ServiceProvider.GetRequiredService<FirstConfig>();

            actual.FirstValue.Should().Be(originalData["First:FirstValue"]);

            source.Update(updatedData);

            actual.FirstValue.Should().Be(originalData["First:FirstValue"]);
        }

        [Fact]
        public void ConfigureWithDoesNotUpdateInterfaceWithUpdatedDataWhenReloadIsDisabled()
        {
            var originalData = new Dictionary<string, string>
            {
                ["RootValue"] = "This is the root value",
                ["First:FirstValue"] = "This is the first value",
                ["First:Id"] = Guid.NewGuid().ToString(),
                ["First:Second:SecondValue"] = "This is the second value",
                ["First:Second:Third:ThirdValue"] = "This is the third value"
            };
            var updatedData = originalData.ToDictionary(x => x.Key, x => Guid.NewGuid().ToString());
            var source = new ReloadSource(originalData);

            var builder = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((_, configuration) => { configuration.Add(source); })
                .ConfigureWith<Config>(false);

            using var host = builder.Build();

            using var scope = host.Services.CreateScope();

            var actual = scope.ServiceProvider.GetRequiredService<IFirstConfig>();

            actual.FirstValue.Should().Be(originalData["First:FirstValue"]);

            source.Update(updatedData);

            actual.FirstValue.Should().Be(originalData["First:FirstValue"]);
        }

        [Theory]
        [InlineData(LogCategoryType.TargetType, "Neovolve.Configuration.DependencyInjection.UnitTests.FirstConfig")]
        [InlineData(LogCategoryType.LibraryType, "Neovolve.Configuration.DependencyInjection.ConfigureWith")]
        [InlineData(LogCategoryType.Custom, "A436F5013F574906BAE263824DEFA140")]
        public void ConfigureWithLogsConfigurationUpdatesBasedOnLogCategoryOptions(LogCategoryType logCategoryType,
            string expectedCategory)
        {
            var originalData = new Dictionary<string, string>
            {
                ["RootValue"] = "This is the root value",
                ["First:FirstValue"] = "This is the first value",
                ["First:Id"] = Guid.NewGuid().ToString(),
                ["First:Second:SecondValue"] = "This is the second value",
                ["First:Second:Third:ThirdValue"] = "This is the third value"
            };
            var updatedData = originalData.ToDictionary(x => x.Key, x => Guid.NewGuid().ToString());
            var source = new ReloadSource(originalData);
            var factory = Substitute.For<ILoggerFactory>();

            var builder = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((_, configuration) => { configuration.Add(source); })
                .ConfigureLogging(x => { x.AddXunit(_output); })
                .ConfigureServices(x => { x.AddSingleton(factory); })
                .ConfigureWith<Config>(x =>
                {
                    x.LogCategoryType = logCategoryType;

                    if (logCategoryType == LogCategoryType.Custom)
                    {
                        x.CustomLogCategory = expectedCategory;
                    }
                });

            using var host = builder.Build();

            using var scope = host.Services.CreateScope();

            var actual = scope.ServiceProvider.GetRequiredService<FirstConfig>();

            actual.FirstValue.Should().Be(originalData["First:FirstValue"]);

            source.Update(updatedData);

            factory.Received(1).CreateLogger(expectedCategory);
        }

        [Fact]
        public void ConfigureWithRegistersConfigTypeReturnsClassWithOriginalData()
        {
            var data = new Dictionary<string, string?>
            {
                ["RootValue"] = "This is the root value",
                ["First:FirstValue"] = "This is the first value",
                ["First:Id"] = Guid.NewGuid().ToString(),
                ["First:Second:SecondValue"] = "This is the second value",
                ["First:Second:Third:ThirdValue"] = "This is the third value"
            };
            var builder = Host.CreateDefaultBuilder();

            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(
                    data);
            });

            builder.ConfigureWith<Config>(true);

            using var host = builder.Build();

            using var scope = host.Services.CreateScope();

            scope.ServiceProvider.GetRequiredService<Config>().RootValue.Should().Be(data["RootValue"]);
            scope.ServiceProvider.GetRequiredService<IConfig>().RootValue.Should().Be(data["RootValue"]);
            scope.ServiceProvider.GetRequiredService<FirstConfig>().FirstValue.Should().Be(data["First:FirstValue"]);
            scope.ServiceProvider.GetRequiredService<FirstConfig>().Id.Should().Be(Guid.Parse(data["First:Id"]));
            scope.ServiceProvider.GetRequiredService<IOptionsMonitor<FirstConfig>>().CurrentValue.FirstValue.Should()
                .Be(data["First:FirstValue"]);
            scope.ServiceProvider.GetRequiredService<IOptionsMonitor<FirstConfig>>().CurrentValue.Id.Should()
                .Be(Guid.Parse(data["First:Id"]));
            scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<FirstConfig>>().Value.FirstValue.Should()
                .Be(data["First:FirstValue"]);
            scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<FirstConfig>>().Value.Id.Should()
                .Be(Guid.Parse(data["First:Id"]));
            scope.ServiceProvider.GetRequiredService<IOptions<FirstConfig>>().Value.FirstValue.Should()
                .Be(data["First:FirstValue"]);
            scope.ServiceProvider.GetRequiredService<IOptions<FirstConfig>>().Value.Id.Should()
                .Be(Guid.Parse(data["First:Id"]));
            scope.ServiceProvider.GetRequiredService<IFirstConfig>().FirstValue.Should().Be(data["First:FirstValue"]);
            scope.ServiceProvider.GetRequiredService<IFirstConfig>().Id.Should().Be(Guid.Parse(data["First:Id"]));
            scope.ServiceProvider.GetRequiredService<IOptionsMonitor<IFirstConfig>>().CurrentValue.FirstValue.Should()
                .Be(data["First:FirstValue"]);
            scope.ServiceProvider.GetRequiredService<IOptionsMonitor<IFirstConfig>>().CurrentValue.Id.Should()
                .Be(Guid.Parse(data["First:Id"]));
            scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<IFirstConfig>>().Value.FirstValue.Should()
                .Be(data["First:FirstValue"]);
            scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<IFirstConfig>>().Value.Id.Should()
                .Be(Guid.Parse(data["First:Id"]));
            scope.ServiceProvider.GetRequiredService<IOptions<IFirstConfig>>().Value.FirstValue.Should()
                .Be(data["First:FirstValue"]);
            scope.ServiceProvider.GetRequiredService<IOptions<IFirstConfig>>().Value.Id.Should()
                .Be(Guid.Parse(data["First:Id"]));
            scope.ServiceProvider.GetRequiredService<SecondConfig>().SecondValue.Should()
                .Be(data["First:Second:SecondValue"]);
            scope.ServiceProvider.GetRequiredService<IOptionsMonitor<SecondConfig>>().CurrentValue.SecondValue.Should()
                .Be(data["First:Second:SecondValue"]);
            scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<SecondConfig>>().Value.SecondValue.Should()
                .Be(data["First:Second:SecondValue"]);
            scope.ServiceProvider.GetRequiredService<IOptions<SecondConfig>>().Value.SecondValue.Should()
                .Be(data["First:Second:SecondValue"]);
            scope.ServiceProvider.GetRequiredService<ISecondConfig>().SecondValue.Should()
                .Be(data["First:Second:SecondValue"]);
            scope.ServiceProvider.GetRequiredService<IOptionsMonitor<ISecondConfig>>().CurrentValue.SecondValue.Should()
                .Be(data["First:Second:SecondValue"]);
            scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<ISecondConfig>>().Value.SecondValue.Should()
                .Be(data["First:Second:SecondValue"]);
            scope.ServiceProvider.GetRequiredService<IOptions<ISecondConfig>>().Value.SecondValue.Should()
                .Be(data["First:Second:SecondValue"]);
            scope.ServiceProvider.GetRequiredService<ThirdConfig>().ThirdValue.Should()
                .Be(data["First:Second:Third:ThirdValue"]);
            scope.ServiceProvider.GetRequiredService<IOptionsMonitor<ThirdConfig>>().CurrentValue.ThirdValue.Should()
                .Be(data["First:Second:Third:ThirdValue"]);
            scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<ThirdConfig>>().Value.ThirdValue.Should()
                .Be(data["First:Second:Third:ThirdValue"]);
            scope.ServiceProvider.GetRequiredService<IOptions<ThirdConfig>>().Value.ThirdValue.Should()
                .Be(data["First:Second:Third:ThirdValue"]);
            scope.ServiceProvider.GetRequiredService<IThirdConfig>().ThirdValue.Should()
                .Be(data["First:Second:Third:ThirdValue"]);
            scope.ServiceProvider.GetRequiredService<IOptionsMonitor<IThirdConfig>>().CurrentValue.ThirdValue.Should()
                .Be(data["First:Second:Third:ThirdValue"]);
            scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<IThirdConfig>>().Value.ThirdValue.Should()
                .Be(data["First:Second:Third:ThirdValue"]);
            scope.ServiceProvider.GetRequiredService<IOptions<IThirdConfig>>().Value.ThirdValue.Should()
                .Be(data["First:Second:Third:ThirdValue"]);
        }

        [Fact]
        public void ConfigureWithUpdatesClassWithUpdatedData()
        {
            var originalData = new Dictionary<string, string>
            {
                ["RootValue"] = "This is the root value",
                ["First:FirstValue"] = "This is the first value",
                ["First:Id"] = Guid.NewGuid().ToString(),
                ["First:Second:SecondValue"] = "This is the second value",
                ["First:Second:Third:ThirdValue"] = "This is the third value"
            };
            var updatedData = originalData.ToDictionary(x => x.Key, x => Guid.NewGuid().ToString());
            var source = new ReloadSource(originalData);

            var builder = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((_, configuration) => { configuration.Add(source); })
                .ConfigureWith<Config>(true);

            using var host = builder.Build();

            using var scope = host.Services.CreateScope();

            var actual = scope.ServiceProvider.GetRequiredService<FirstConfig>();

            actual.FirstValue.Should().Be(originalData["First:FirstValue"]);

            source.Update(updatedData);

            scope.ServiceProvider.GetRequiredService<FirstConfig>().FirstValue.Should()
                .Be(updatedData["First:FirstValue"]);
            scope.ServiceProvider.GetRequiredService<FirstConfig>().Id.Should().Be(Guid.Parse(updatedData["First:Id"]));
            scope.ServiceProvider.GetRequiredService<IOptionsMonitor<FirstConfig>>().CurrentValue.FirstValue.Should()
                .Be(updatedData["First:FirstValue"]);
            scope.ServiceProvider.GetRequiredService<IOptionsMonitor<FirstConfig>>().CurrentValue.Id.Should()
                .Be(Guid.Parse(updatedData["First:Id"]));
            scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<FirstConfig>>().Value.FirstValue.Should()
                .Be(updatedData["First:FirstValue"]);
            scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<FirstConfig>>().Value.Id.Should()
                .Be(Guid.Parse(updatedData["First:Id"]));
            scope.ServiceProvider.GetRequiredService<IOptions<FirstConfig>>().Value.FirstValue.Should()
                .Be(updatedData["First:FirstValue"]);
            scope.ServiceProvider.GetRequiredService<IOptions<FirstConfig>>().Value.Id.Should()
                .Be(Guid.Parse(updatedData["First:Id"]));
            scope.ServiceProvider.GetRequiredService<IFirstConfig>().FirstValue.Should()
                .Be(updatedData["First:FirstValue"]);
            scope.ServiceProvider.GetRequiredService<IFirstConfig>().Id.Should()
                .Be(Guid.Parse(updatedData["First:Id"]));
            scope.ServiceProvider.GetRequiredService<IOptionsMonitor<IFirstConfig>>().CurrentValue.FirstValue.Should()
                .Be(updatedData["First:FirstValue"]);
            scope.ServiceProvider.GetRequiredService<IOptionsMonitor<IFirstConfig>>().CurrentValue.Id.Should()
                .Be(Guid.Parse(updatedData["First:Id"]));
            scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<IFirstConfig>>().Value.FirstValue.Should()
                .Be(updatedData["First:FirstValue"]);
            scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<IFirstConfig>>().Value.Id.Should()
                .Be(Guid.Parse(updatedData["First:Id"]));
            scope.ServiceProvider.GetRequiredService<IOptions<IFirstConfig>>().Value.FirstValue.Should()
                .Be(updatedData["First:FirstValue"]);
            scope.ServiceProvider.GetRequiredService<IOptions<IFirstConfig>>().Value.Id.Should()
                .Be(Guid.Parse(updatedData["First:Id"]));
            scope.ServiceProvider.GetRequiredService<SecondConfig>().SecondValue.Should()
                .Be(updatedData["First:Second:SecondValue"]);
            scope.ServiceProvider.GetRequiredService<IOptionsMonitor<SecondConfig>>().CurrentValue.SecondValue.Should()
                .Be(updatedData["First:Second:SecondValue"]);
            scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<SecondConfig>>().Value.SecondValue.Should()
                .Be(updatedData["First:Second:SecondValue"]);
            scope.ServiceProvider.GetRequiredService<IOptions<SecondConfig>>().Value.SecondValue.Should()
                .Be(updatedData["First:Second:SecondValue"]);
            scope.ServiceProvider.GetRequiredService<ISecondConfig>().SecondValue.Should()
                .Be(updatedData["First:Second:SecondValue"]);
            scope.ServiceProvider.GetRequiredService<IOptionsMonitor<ISecondConfig>>().CurrentValue.SecondValue.Should()
                .Be(updatedData["First:Second:SecondValue"]);
            scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<ISecondConfig>>().Value.SecondValue.Should()
                .Be(updatedData["First:Second:SecondValue"]);
            scope.ServiceProvider.GetRequiredService<IOptions<ISecondConfig>>().Value.SecondValue.Should()
                .Be(updatedData["First:Second:SecondValue"]);
            scope.ServiceProvider.GetRequiredService<ThirdConfig>().ThirdValue.Should()
                .Be(updatedData["First:Second:Third:ThirdValue"]);
            scope.ServiceProvider.GetRequiredService<IOptionsMonitor<ThirdConfig>>().CurrentValue.ThirdValue.Should()
                .Be(updatedData["First:Second:Third:ThirdValue"]);
            scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<ThirdConfig>>().Value.ThirdValue.Should()
                .Be(updatedData["First:Second:Third:ThirdValue"]);
            scope.ServiceProvider.GetRequiredService<IOptions<ThirdConfig>>().Value.ThirdValue.Should()
                .Be(updatedData["First:Second:Third:ThirdValue"]);
            scope.ServiceProvider.GetRequiredService<IThirdConfig>().ThirdValue.Should()
                .Be(updatedData["First:Second:Third:ThirdValue"]);
            scope.ServiceProvider.GetRequiredService<IOptionsMonitor<IThirdConfig>>().CurrentValue.ThirdValue.Should()
                .Be(updatedData["First:Second:Third:ThirdValue"]);
            scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<IThirdConfig>>().Value.ThirdValue.Should()
                .Be(updatedData["First:Second:Third:ThirdValue"]);
            scope.ServiceProvider.GetRequiredService<IOptions<IThirdConfig>>().Value.ThirdValue.Should()
                .Be(updatedData["First:Second:Third:ThirdValue"]);
        }

        [Fact]
        public void ConfigureWithUpdatesClassWithUpdatedDataOnSecondGetService()
        {
            var originalData = new Dictionary<string, string>
            {
                ["RootValue"] = "This is the root value",
                ["First:FirstValue"] = "This is the first value",
                ["First:Id"] = Guid.NewGuid().ToString(),
                ["First:Second:SecondValue"] = "This is the second value",
                ["First:Second:Third:ThirdValue"] = "This is the third value"
            };
            var updatedData = originalData.ToDictionary(x => x.Key, x => Guid.NewGuid().ToString());
            var source = new ReloadSource(originalData);

            var builder = Host.CreateDefaultBuilder()
                .ConfigureLogging(x =>
                {
                    x.SetMinimumLevel(LogLevel.Debug);
                    x.AddXunit(_output, new LoggingConfig { LogLevel = LogLevel.Debug });
                })
                .ConfigureAppConfiguration((_, configuration) => { configuration.Add(source); })
                .ConfigureWith<Config>(true);

            using var host = builder.Build();

            using var scope = host.Services.CreateScope();

            var firstActual = scope.ServiceProvider.GetRequiredService<FirstConfig>();

            firstActual.FirstValue.Should().Be(originalData["First:FirstValue"]);

            source.Update(updatedData);

            var secondActual = scope.ServiceProvider.GetRequiredService<FirstConfig>();

            secondActual.FirstValue.Should().Be(updatedData["First:FirstValue"]);
        }

        [Fact]
        public void ConfigureWithUpdatesClassWithUpdatedDataOnServiceResolvedAfterUpdate()
        {
            var originalData = new Dictionary<string, string>
            {
                ["RootValue"] = "This is the root value",
                ["First:FirstValue"] = "This is the first value",
                ["First:Id"] = Guid.NewGuid().ToString(),
                ["First:Second:SecondValue"] = "This is the second value",
                ["First:Second:Third:ThirdValue"] = "This is the third value"
            };
            var updatedData = originalData.ToDictionary(x => x.Key, x => Guid.NewGuid().ToString());
            var source = new ReloadSource(originalData);

            var builder = Host.CreateDefaultBuilder()
                .ConfigureLogging(x =>
                {
                    x.SetMinimumLevel(LogLevel.Debug);
                    x.AddXunit(_output, new LoggingConfig { LogLevel = LogLevel.Debug });
                })
                .ConfigureAppConfiguration((_, configuration) => { configuration.Add(source); })
                .ConfigureWith<Config>(true);

            using var host = builder.Build();

            using var scope = host.Services.CreateScope();

            var firstActual = scope.ServiceProvider.GetRequiredService<FirstConfig>();

            firstActual.FirstValue.Should().Be(originalData["First:FirstValue"]);

            source.Update(updatedData);

            var secondActual = scope.ServiceProvider.GetRequiredService<SecondConfig>();

            secondActual.SecondValue.Should().Be(updatedData["First:Second:SecondValue"]);
        }

        [Fact]
        public void ConfigureWithUpdatesInterfaceWithUpdatedData()
        {
            var originalData = new Dictionary<string, string>
            {
                ["RootValue"] = "This is the root value",
                ["First:FirstValue"] = "This is the first value",
                ["First:Id"] = Guid.NewGuid().ToString(),
                ["First:Second:SecondValue"] = "This is the second value",
                ["First:Second:Third:ThirdValue"] = "This is the third value"
            };
            var updatedData = originalData.ToDictionary(x => x.Key, x => Guid.NewGuid().ToString());
            var source = new ReloadSource(originalData);

            var builder = Host.CreateDefaultBuilder()
                .ConfigureLogging(x =>
                {
                    x.SetMinimumLevel(LogLevel.Debug);
                    x.AddXunit(_output, new LoggingConfig { LogLevel = LogLevel.Debug });
                })
                .ConfigureAppConfiguration((_, configuration) => { configuration.Add(source); })
                .ConfigureWith<Config>(true);

            using var host = builder.Build();

            using var scope = host.Services.CreateScope();

            var actual = scope.ServiceProvider.GetRequiredService<IFirstConfig>();

            actual.FirstValue.Should().Be(originalData["First:FirstValue"]);

            source.Update(updatedData);

            scope.ServiceProvider.GetRequiredService<FirstConfig>().FirstValue.Should()
                .Be(updatedData["First:FirstValue"]);
            scope.ServiceProvider.GetRequiredService<FirstConfig>().Id.Should().Be(Guid.Parse(updatedData["First:Id"]));
            scope.ServiceProvider.GetRequiredService<IOptionsMonitor<FirstConfig>>().CurrentValue.FirstValue.Should()
                .Be(updatedData["First:FirstValue"]);
            scope.ServiceProvider.GetRequiredService<IOptionsMonitor<FirstConfig>>().CurrentValue.Id.Should()
                .Be(Guid.Parse(updatedData["First:Id"]));
            scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<FirstConfig>>().Value.FirstValue.Should()
                .Be(updatedData["First:FirstValue"]);
            scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<FirstConfig>>().Value.Id.Should()
                .Be(Guid.Parse(updatedData["First:Id"]));
            scope.ServiceProvider.GetRequiredService<IOptions<FirstConfig>>().Value.FirstValue.Should()
                .Be(updatedData["First:FirstValue"]);
            scope.ServiceProvider.GetRequiredService<IOptions<FirstConfig>>().Value.Id.Should()
                .Be(Guid.Parse(updatedData["First:Id"]));
            scope.ServiceProvider.GetRequiredService<IFirstConfig>().FirstValue.Should()
                .Be(updatedData["First:FirstValue"]);
            scope.ServiceProvider.GetRequiredService<IFirstConfig>().Id.Should()
                .Be(Guid.Parse(updatedData["First:Id"]));
            scope.ServiceProvider.GetRequiredService<IOptionsMonitor<IFirstConfig>>().CurrentValue.FirstValue.Should()
                .Be(updatedData["First:FirstValue"]);
            scope.ServiceProvider.GetRequiredService<IOptionsMonitor<IFirstConfig>>().CurrentValue.Id.Should()
                .Be(Guid.Parse(updatedData["First:Id"]));
            scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<IFirstConfig>>().Value.FirstValue.Should()
                .Be(updatedData["First:FirstValue"]);
            scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<IFirstConfig>>().Value.Id.Should()
                .Be(Guid.Parse(updatedData["First:Id"]));
            scope.ServiceProvider.GetRequiredService<IOptions<IFirstConfig>>().Value.FirstValue.Should()
                .Be(updatedData["First:FirstValue"]);
            scope.ServiceProvider.GetRequiredService<IOptions<IFirstConfig>>().Value.Id.Should()
                .Be(Guid.Parse(updatedData["First:Id"]));
            scope.ServiceProvider.GetRequiredService<SecondConfig>().SecondValue.Should()
                .Be(updatedData["First:Second:SecondValue"]);
            scope.ServiceProvider.GetRequiredService<IOptionsMonitor<SecondConfig>>().CurrentValue.SecondValue.Should()
                .Be(updatedData["First:Second:SecondValue"]);
            scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<SecondConfig>>().Value.SecondValue.Should()
                .Be(updatedData["First:Second:SecondValue"]);
            scope.ServiceProvider.GetRequiredService<IOptions<SecondConfig>>().Value.SecondValue.Should()
                .Be(updatedData["First:Second:SecondValue"]);
            scope.ServiceProvider.GetRequiredService<ISecondConfig>().SecondValue.Should()
                .Be(updatedData["First:Second:SecondValue"]);
            scope.ServiceProvider.GetRequiredService<IOptionsMonitor<ISecondConfig>>().CurrentValue.SecondValue.Should()
                .Be(updatedData["First:Second:SecondValue"]);
            scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<ISecondConfig>>().Value.SecondValue.Should()
                .Be(updatedData["First:Second:SecondValue"]);
            scope.ServiceProvider.GetRequiredService<IOptions<ISecondConfig>>().Value.SecondValue.Should()
                .Be(updatedData["First:Second:SecondValue"]);
            scope.ServiceProvider.GetRequiredService<ThirdConfig>().ThirdValue.Should()
                .Be(updatedData["First:Second:Third:ThirdValue"]);
            scope.ServiceProvider.GetRequiredService<IOptionsMonitor<ThirdConfig>>().CurrentValue.ThirdValue.Should()
                .Be(updatedData["First:Second:Third:ThirdValue"]);
            scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<ThirdConfig>>().Value.ThirdValue.Should()
                .Be(updatedData["First:Second:Third:ThirdValue"]);
            scope.ServiceProvider.GetRequiredService<IOptions<ThirdConfig>>().Value.ThirdValue.Should()
                .Be(updatedData["First:Second:Third:ThirdValue"]);
            scope.ServiceProvider.GetRequiredService<IThirdConfig>().ThirdValue.Should()
                .Be(updatedData["First:Second:Third:ThirdValue"]);
            scope.ServiceProvider.GetRequiredService<IOptionsMonitor<IThirdConfig>>().CurrentValue.ThirdValue.Should()
                .Be(updatedData["First:Second:Third:ThirdValue"]);
            scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<IThirdConfig>>().Value.ThirdValue.Should()
                .Be(updatedData["First:Second:Third:ThirdValue"]);
            scope.ServiceProvider.GetRequiredService<IOptions<IThirdConfig>>().Value.ThirdValue.Should()
                .Be(updatedData["First:Second:Third:ThirdValue"]);
        }

        [Fact]
        public void ConfigureWithUpdatesInterfaceWithUpdatedDataOnSecondGetService()
        {
            var originalData = new Dictionary<string, string>
            {
                ["RootValue"] = "This is the root value",
                ["First:FirstValue"] = "This is the first value",
                ["First:Id"] = Guid.NewGuid().ToString(),
                ["First:Second:SecondValue"] = "This is the second value",
                ["First:Second:Third:ThirdValue"] = "This is the third value"
            };
            var updatedData = originalData.ToDictionary(x => x.Key, x => Guid.NewGuid().ToString());
            var source = new ReloadSource(originalData);

            var builder = Host.CreateDefaultBuilder()
                .ConfigureLogging(x =>
                {
                    x.SetMinimumLevel(LogLevel.Debug);
                    x.AddXunit(_output, new LoggingConfig { LogLevel = LogLevel.Debug });
                })
                .ConfigureAppConfiguration((_, configuration) => { configuration.Add(source); })
                .ConfigureWith<Config>(true);

            using var host = builder.Build();

            using var scope = host.Services.CreateScope();

            var firstActual = scope.ServiceProvider.GetRequiredService<IFirstConfig>();

            firstActual.FirstValue.Should().Be(originalData["First:FirstValue"]);

            source.Update(updatedData);

            var secondActual = scope.ServiceProvider.GetRequiredService<IFirstConfig>();

            secondActual.FirstValue.Should().Be(updatedData["First:FirstValue"]);
        }

        [Fact]
        public void ConfigureWithUpdatesInterfaceWithUpdatedDataOnServiceResolvedAfterUpdate()
        {
            var originalData = new Dictionary<string, string>
            {
                ["RootValue"] = "This is the root value",
                ["First:FirstValue"] = "This is the first value",
                ["First:Id"] = Guid.NewGuid().ToString(),
                ["First:Second:SecondValue"] = "This is the second value",
                ["First:Second:Third:ThirdValue"] = "This is the third value"
            };
            var updatedData = originalData.ToDictionary(x => x.Key, x => Guid.NewGuid().ToString());
            var source = new ReloadSource(originalData);

            var builder = Host.CreateDefaultBuilder()
                .ConfigureLogging(x =>
                {
                    x.SetMinimumLevel(LogLevel.Debug);
                    x.AddXunit(_output, new LoggingConfig { LogLevel = LogLevel.Debug });
                })
                .ConfigureAppConfiguration((_, configuration) => { configuration.Add(source); })
                .ConfigureWith<Config>(true);

            using var host = builder.Build();

            using var scope = host.Services.CreateScope();

            var firstActual = scope.ServiceProvider.GetRequiredService<IFirstConfig>();

            firstActual.FirstValue.Should().Be(originalData["First:FirstValue"]);

            source.Update(updatedData);

            var secondActual = scope.ServiceProvider.GetRequiredService<ISecondConfig>();

            secondActual.SecondValue.Should().Be(updatedData["First:Second:SecondValue"]);
        }

        [Fact]
        public void ConfigureWithUpdatesMonitorInterfaceWithUpdatedData()
        {
            var originalData = new Dictionary<string, string>
            {
                ["RootValue"] = "This is the root value",
                ["First:FirstValue"] = "This is the first value",
                ["First:Id"] = Guid.NewGuid().ToString(),
                ["First:Second:SecondValue"] = "This is the second value",
                ["First:Second:Third:ThirdValue"] = "This is the third value"
            };
            var updatedData = originalData.ToDictionary(x => x.Key, x => Guid.NewGuid().ToString());
            var source = new ReloadSource(originalData);

            var builder = Host.CreateDefaultBuilder()
                .ConfigureLogging(x =>
                {
                    x.SetMinimumLevel(LogLevel.Debug);
                    x.AddXunit(_output, new LoggingConfig { LogLevel = LogLevel.Debug });
                })
                .ConfigureAppConfiguration((_, configuration) => { configuration.Add(source); })
                .ConfigureWith<Config>(true);

            using var host = builder.Build();

            using var scope = host.Services.CreateScope();

            var actual = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<IFirstConfig>>();

            actual.CurrentValue.FirstValue.Should().Be(originalData["First:FirstValue"]);

            source.Update(updatedData);

            actual.CurrentValue.FirstValue.Should().Be(updatedData["First:FirstValue"]);
        }

        [Fact]
        public void ConfigureWithUpdatesMonitorInterfaceWithUpdatedDataOnSecondGetService()
        {
            var originalData = new Dictionary<string, string>
            {
                ["RootValue"] = "This is the root value",
                ["First:FirstValue"] = "This is the first value",
                ["First:Id"] = Guid.NewGuid().ToString(),
                ["First:Second:SecondValue"] = "This is the second value",
                ["First:Second:Third:ThirdValue"] = "This is the third value"
            };
            var updatedData = originalData.ToDictionary(x => x.Key, x => Guid.NewGuid().ToString());
            var source = new ReloadSource(originalData);

            var builder = Host.CreateDefaultBuilder()
                .ConfigureLogging(x =>
                {
                    x.SetMinimumLevel(LogLevel.Debug);
                    x.AddXunit(_output, new LoggingConfig { LogLevel = LogLevel.Debug });
                })
                .ConfigureAppConfiguration((_, configuration) => { configuration.Add(source); })
                .ConfigureWith<Config>(true);

            using var host = builder.Build();

            using var scope = host.Services.CreateScope();

            var firstActual = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<IFirstConfig>>();

            firstActual.CurrentValue.FirstValue.Should().Be(originalData["First:FirstValue"]);

            source.Update(updatedData);

            var secondActual = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<IFirstConfig>>();

            secondActual.CurrentValue.FirstValue.Should().Be(updatedData["First:FirstValue"]);
        }

        [Theory]
        [InlineData(typeof(Config), true)]
        [InlineData(typeof(IConfig), true)]
        [InlineData(typeof(FirstConfig), true)]
        [InlineData(typeof(IOptionsMonitor<FirstConfig>), true)]
        [InlineData(typeof(IOptionsSnapshot<FirstConfig>), true)]
        [InlineData(typeof(IOptions<FirstConfig>), true)]
        [InlineData(typeof(IFirstConfig), true)]
        [InlineData(typeof(IOptionsMonitor<IFirstConfig>), true)]
        [InlineData(typeof(IOptionsSnapshot<IFirstConfig>), true)]
        [InlineData(typeof(IOptions<IFirstConfig>), true)]
        [InlineData(typeof(SecondConfig), true)]
        [InlineData(typeof(IOptionsMonitor<SecondConfig>), true)]
        [InlineData(typeof(IOptionsSnapshot<SecondConfig>), true)]
        [InlineData(typeof(IOptions<SecondConfig>), true)]
        [InlineData(typeof(ISecondConfig), true)]
        [InlineData(typeof(IOptionsMonitor<ISecondConfig>), true)]
        [InlineData(typeof(IOptionsSnapshot<ISecondConfig>), true)]
        [InlineData(typeof(IOptions<ISecondConfig>), true)]
        [InlineData(typeof(ThirdConfig), true)]
        [InlineData(typeof(IOptionsMonitor<ThirdConfig>), true)]
        [InlineData(typeof(IOptionsSnapshot<ThirdConfig>), true)]
        [InlineData(typeof(IOptions<ThirdConfig>), true)]
        [InlineData(typeof(IThirdConfig), true)]
        [InlineData(typeof(IOptionsMonitor<IThirdConfig>), true)]
        [InlineData(typeof(IOptionsSnapshot<IThirdConfig>), true)]
        [InlineData(typeof(IOptions<IThirdConfig>), true)]
        public void RegisterConfigTypeRegistersTypesWithExpectedLifecycle(Type configType, bool isSingleton)
        {
            var data = new Dictionary<string, string?>
            {
                ["RootValue"] = "This is the root value",
                ["First:FirstValue"] = "This is the first value",
                ["First:Id"] = Guid.NewGuid().ToString(),
                ["First:Second:SecondValue"] = "This is the second value",
                ["First:Second:Third:ThirdValue"] = "This is the third value"
            };
            var builder = Host.CreateDefaultBuilder();

            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(
                    data);
            });

            builder.ConfigureWith<Config>(true);

            using var host = builder.Build();

            using var scope = host.Services.CreateScope();

            var provider = configType.Name.Contains("Snapshot") ? scope.ServiceProvider : host.Services;

            var firstActual = provider.GetRequiredService(configType);
            var secondActual = provider.GetRequiredService(configType);

            if (isSingleton)
            {
                firstActual.Should().BeSameAs(secondActual);
            }
            else
            {
                firstActual.Should().NotBeSameAs(secondActual);
            }
        }
    }
}