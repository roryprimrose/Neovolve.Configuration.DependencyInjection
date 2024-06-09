namespace Neovolve.Configuration.DependencyInjection.UnitTests.Comparison
{
    using FluentAssertions;
    using Neovolve.Configuration.DependencyInjection.Comparison;

    public class IdentifiedChangeTests
    {
        [Fact]
        public void CanCreateInstance()
        {
            var propertyPath = Guid.NewGuid().ToString();
            var firstLogValue = Guid.NewGuid().ToString();
            var secondLogValue = Guid.NewGuid().ToString();

            var actual = new IdentifiedChange(propertyPath, firstLogValue, secondLogValue);

            actual.PropertyPath.Should().Be(propertyPath);
            actual.FirstLogValue.Should().Be(firstLogValue);
            actual.SecondLogValue.Should().Be(secondLogValue);
            actual.MessageFormat.Should()
                .Be(IdentifiedChange.DefaultMessageFormatPrefix + IdentifiedChange.DefaultMessageFormat);
        }

        [Fact]
        public void CanCreateInstanceWithMessageFormat()
        {
            var propertyPath = Guid.NewGuid().ToString();
            var firstLogValue = Guid.NewGuid().ToString();
            var secondLogValue = Guid.NewGuid().ToString();
            var messageFormat = $"something '{IdentifiedChange.OldValueMask}' here '{IdentifiedChange.NewValueMask}'";

            var actual = new IdentifiedChange(propertyPath, firstLogValue, secondLogValue, messageFormat);

            actual.PropertyPath.Should().Be(propertyPath);
            actual.FirstLogValue.Should().Be(firstLogValue);
            actual.SecondLogValue.Should().Be(secondLogValue);
            actual.MessageFormat.Should().Be(IdentifiedChange.DefaultMessageFormatPrefix + messageFormat);
        }

        [Theory]
        [InlineData($"Something {IdentifiedChange.NewValueMask}")]
        [InlineData($"Something {IdentifiedChange.OldValueMask}")]
        public void CreateThrowsExceptionWhenSpecifiedMessageFormatLacksPlaceholders(string messageFormat)
        {
            var propertyPath = Guid.NewGuid().ToString();
            var firstLogValue = Guid.NewGuid().ToString();
            var secondLogValue = Guid.NewGuid().ToString();

            var action = () => new IdentifiedChange(propertyPath, firstLogValue, secondLogValue, messageFormat);

            action.Should().Throw<ArgumentException>().And.ParamName.Should().Be("messageFormat");
        }
    }
}