namespace Neovolve.Configuration.DependencyInjection.IntegrationTests
{
    using FluentAssertions;
    using WebTestHost;
    using Xunit;
    using Xunit.Abstractions;

    public class BasicTests
        : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly ITestOutputHelper _output;

        public BasicTests(CustomWebApplicationFactory<Program> factory, ITestOutputHelper output)
        {
            _factory = factory;
            _output = output;
        }

        [Theory]
        [InlineData("/ConcreteConfig")]
        [InlineData("/IntefaceConfig")]
        [InlineData("/ConcreteFirst")]
        [InlineData("/IntefaceFirst")]
        [InlineData("/ConcreteSecon")]
        [InlineData("/IntefaceSecond")]
        [InlineData("/ConcreteThird")]
        [InlineData("/IntefaceThird")]
        public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url)
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync(url);

            // Assert
            var content = await response.Content.ReadAsStringAsync();

            _output.WriteLine(content);

            response.EnsureSuccessStatusCode(); // Status Code 200-299

            response.Content.Headers.ContentType.ToString().Should().Be("application/json; charset=utf-8");
        }
    }
}