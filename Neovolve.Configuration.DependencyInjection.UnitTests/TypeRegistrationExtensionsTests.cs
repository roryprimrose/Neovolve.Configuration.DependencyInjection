namespace Neovolve.Configuration.DependencyInjection.UnitTests
{
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Options;

    public class TypeRegistrationExtensionsTests
    {
        [Fact]
        public void ConfigureWithRegistersConfigTypeReturnsClassWithOriginalData()
        {
            var data = new Dictionary<string, string?>
            {
                ["RootValue"] = "This is the root value",
                ["First:FirstValue"] = "This is the first value",
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
            scope.ServiceProvider.GetRequiredService<IOptionsMonitor<FirstConfig>>().CurrentValue.FirstValue.Should()
                .Be(data["First:FirstValue"]);
            scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<FirstConfig>>().Value.FirstValue.Should()
                .Be(data["First:FirstValue"]);
            scope.ServiceProvider.GetRequiredService<IOptions<FirstConfig>>().Value.FirstValue.Should()
                .Be(data["First:FirstValue"]);
            scope.ServiceProvider.GetRequiredService<IFirstConfig>().FirstValue.Should().Be(data["First:FirstValue"]);
            scope.ServiceProvider.GetRequiredService<IOptionsMonitor<IFirstConfig>>().CurrentValue.FirstValue.Should()
                .Be(data["First:FirstValue"]);
            scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<IFirstConfig>>().Value.FirstValue.Should()
                .Be(data["First:FirstValue"]);
            scope.ServiceProvider.GetRequiredService<IOptions<IFirstConfig>>().Value.FirstValue.Should()
                .Be(data["First:FirstValue"]);
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

        [Fact(Skip = "This does not work yet")]
        public void ConfigureWithRegistersConfigTypeReturnsClassWithUpdatedData()
        {
            var originalData = new Dictionary<string, string?>
            {
                ["RootValue"] = "This is the root value",
                ["First:FirstValue"] = "This is the first value",
                ["First:Second:SecondValue"] = "This is the second value",
                ["First:Second:Third:ThirdValue"] = "This is the third value"
            };
            var updatedData = originalData.ToDictionary(x => x.Key, x => x.Value + "-" + Guid.NewGuid());
            var builder = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((_, configuration) => { configuration.AddInMemoryCollection(originalData); })
                .ConfigureWith<Config>(true);

            using var host = builder.Build();

            var reloadToken =
                host.Services.GetRequiredService<IConfiguration>().GetReloadToken() as ConfigurationReloadToken;

            using var scope = host.Services.CreateScope();

            var firstConfig = scope.ServiceProvider.GetRequiredService<FirstConfig>();

            firstConfig.FirstValue.Should().Be(originalData["First:FirstValue"]);

            // Copy new values to the original data
            foreach (var originalDataKey in originalData.Keys)
            {
                originalData[originalDataKey] = updatedData[originalDataKey];
            }

            reloadToken!.OnReload();

            firstConfig.FirstValue.Should().Be(updatedData["First:FirstValue"]);

            //scope.ServiceProvider.GetRequiredService<Config>().RootValue.Should().Be(originalData["RootValue"]);
            //scope.ServiceProvider.GetRequiredService<IConfig>().RootValue.Should().Be(originalData["RootValue"]);
            //scope.ServiceProvider.GetRequiredService<IOptionsMonitor<FirstConfig>>().CurrentValue.FirstValue.Should().Be(updateData["First:FirstValue"]);
            //scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<FirstConfig>>().Value.FirstValue.Should().Be(updateData["First:FirstValue"]);
            //scope.ServiceProvider.GetRequiredService<IOptions<FirstConfig>>().Value.FirstValue.Should().Be(originalData["First:FirstValue"]);
            //scope.ServiceProvider.GetRequiredService<IFirstConfig>().FirstValue.Should().Be(updateData["First:FirstValue"]);
            //scope.ServiceProvider.GetRequiredService<IOptionsMonitor<IFirstConfig>>().CurrentValue.FirstValue.Should().Be(updateData["First:FirstValue"]);
            //scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<IFirstConfig>>().Value.FirstValue.Should().Be(updateData["First:FirstValue"]);
            //scope.ServiceProvider.GetRequiredService<IOptions<IFirstConfig>>().Value.FirstValue.Should().Be(originalData["First:FirstValue"]);
            //scope.ServiceProvider.GetRequiredService<SecondConfig>().SecondValue.Should().Be(updateData["First:Second:SecondValue"]);
            //scope.ServiceProvider.GetRequiredService<IOptionsMonitor<SecondConfig>>().CurrentValue.SecondValue.Should().Be(updateData["First:Second:SecondValue"]);
            //scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<SecondConfig>>().Value.SecondValue.Should().Be(updateData["First:Second:SecondValue"]);
            //scope.ServiceProvider.GetRequiredService<IOptions<SecondConfig>>().Value.SecondValue.Should().Be(originalData["First:Second:SecondValue"]);
            //scope.ServiceProvider.GetRequiredService<ISecondConfig>().SecondValue.Should().Be(updateData["First:Second:SecondValue"]);
            //scope.ServiceProvider.GetRequiredService<IOptionsMonitor<ISecondConfig>>().CurrentValue.SecondValue.Should().Be(updateData["First:Second:SecondValue"]);
            //scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<ISecondConfig>>().Value.SecondValue.Should().Be(updateData["First:Second:SecondValue"]);
            //scope.ServiceProvider.GetRequiredService<IOptions<ISecondConfig>>().Value.SecondValue.Should().Be(originalData["First:Second:SecondValue"]);
            //scope.ServiceProvider.GetRequiredService<ThirdConfig>().ThirdValue.Should().Be(updateData["First:Second:Third:ThirdValue"]);
            //scope.ServiceProvider.GetRequiredService<IOptionsMonitor<ThirdConfig>>().CurrentValue.ThirdValue.Should().Be(updateData["First:Second:Third:ThirdValue"]);
            //scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<ThirdConfig>>().Value.ThirdValue.Should().Be(updateData["First:Second:Third:ThirdValue"]);
            //scope.ServiceProvider.GetRequiredService<IOptions<ThirdConfig>>().Value.ThirdValue.Should().Be(originalData["First:Second:Third:ThirdValue"]);
            //scope.ServiceProvider.GetRequiredService<IThirdConfig>().ThirdValue.Should().Be(updateData["First:Second:Third:ThirdValue"]);
            //scope.ServiceProvider.GetRequiredService<IOptionsMonitor<IThirdConfig>>().CurrentValue.ThirdValue.Should().Be(updateData["First:Second:Third:ThirdValue"]);
            //scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<IThirdConfig>>().Value.ThirdValue.Should().Be(updateData["First:Second:Third:ThirdValue"]);
            //scope.ServiceProvider.GetRequiredService<IOptions<IThirdConfig>>().Value.ThirdValue.Should().Be(originalData["First:Second:Third:ThirdValue"]);
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