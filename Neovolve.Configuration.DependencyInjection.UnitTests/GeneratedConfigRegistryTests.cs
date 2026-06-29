namespace Neovolve.Configuration.DependencyInjection.UnitTests;

using FluentAssertions;
using NSubstitute;
using Neovolve.Configuration.DependencyInjection.Generated;
using Xunit;

public class GeneratedConfigRegistryTests
{
    [Fact]
    public void RegisterGraphThrowsWithNullRegistrar()
    {
        var action = () => GeneratedConfigRegistry.RegisterGraph(typeof(Target), null!);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RegisterGraphThrowsWithNullRootType()
    {
        var action = () => GeneratedConfigRegistry.RegisterGraph(null!, Substitute.For<IConfigGraphRegistrar>());

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RegisterApplierThrowsWithNullApplier()
    {
        var action = () => GeneratedConfigRegistry.RegisterApplier(typeof(Target), null!);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RegisterApplierThrowsWithNullConfigType()
    {
        var action = () => GeneratedConfigRegistry.RegisterApplier(null!, new object());

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TryGetApplierReturnsFalseWhenNotRegistered()
    {
        GeneratedConfigRegistry.TryGetApplier<UnregisteredTarget>(out var applier).Should().BeFalse();
        applier.Should().BeNull();
    }

    [Fact]
    public void TryGetApplierReturnsRegisteredApplier()
    {
        var applier = new TargetApplier();

        GeneratedConfigRegistry.RegisterApplier(typeof(Target), applier);

        GeneratedConfigRegistry.TryGetApplier<Target>(out var actual).Should().BeTrue();
        actual.Should().BeSameAs(applier);
    }

    [Fact]
    public void TryGetApplierThrowsWithNullConfigType()
    {
        var action = () => GeneratedConfigRegistry.TryGetApplier(null!, out _);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TryGetRegistrarThrowsWithNullRootType()
    {
        var action = () => GeneratedConfigRegistry.TryGetRegistrar(null!, out _);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TryGetRegistrarReturnsRegisteredRegistrar()
    {
        var registrar = Substitute.For<IConfigGraphRegistrar>();

        GeneratedConfigRegistry.RegisterGraph(typeof(Target), registrar);

        GeneratedConfigRegistry.TryGetRegistrar(typeof(Target), out var actual).Should().BeTrue();
        actual.Should().BeSameAs(registrar);
    }

    private sealed class Target
    {
    }

    private sealed class UnregisteredTarget
    {
    }

    private sealed class TargetApplier : IConfigValueApplier<Target>
    {
        public bool Apply(Target injected, Target updated, IConfigUpdateContext context)
        {
            return false;
        }
    }
}
