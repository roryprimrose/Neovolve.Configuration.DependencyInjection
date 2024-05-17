namespace Neovolve.Configuration.DependencyInjection.UnitTests.Comparison
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using ModelBuilder;
    using Neovolve.Configuration.DependencyInjection.Comparison;
    using NSubstitute;

    public class TypedChangeEvaluatorTests
    {
        [Fact]
        public void FindChangesCallsNextForUnsupportedType()
        {
            var expectedPath = Guid.NewGuid().ToString();
            var first = Guid.NewGuid().ToString();
            var second = first;
            var fallbackResults = Model.Create<List<IdentifiedChange>>();
            var typeMatchResults = Model.Create<List<IdentifiedChange>>();
            var nextCalled = false;

            NextFindChanges next = (path, firstValue, secondValue) =>
            {
                path.Should().Be(expectedPath);
                firstValue.Should().Be(first);
                secondValue.Should().Be(second);
                nextCalled = true;

                return fallbackResults;
            };
            
            var sut = new Wrapper(typeMatchResults);

            var actual = sut.FindChanges(expectedPath, first, second, next);

            nextCalled.Should().BeTrue();
            actual.Should().BeEquivalentTo(fallbackResults);
        }

        [Fact]
        public void FindChangesReturnsSpecificLogicForSupportedType()
        {
            var expectedPath = Guid.NewGuid().ToString();
            var first = Guid.NewGuid();
            var second = first;
            var fallbackResults = Model.Create<List<IdentifiedChange>>();
            var typeMatchResults = Model.Create<List<IdentifiedChange>>();
            var nextCalled = false;

            NextFindChanges next = (path, firstValue, secondValue) =>
            {
                nextCalled = true;

                return fallbackResults;
            };

            var sut = new Wrapper(typeMatchResults);

            var actual = sut.FindChanges(expectedPath, first, second, next);

            nextCalled.Should().BeFalse();
            actual.Should().BeEquivalentTo(typeMatchResults);
        }

        private class Wrapper : TypedChangeEvaluator<Guid>
        {
            private readonly IEnumerable<IdentifiedChange> _results;

            public Wrapper(IEnumerable<IdentifiedChange> results)
            {
                _results = results;
            }

            protected override IEnumerable<IdentifiedChange> FindChanges(string propertyPath, Guid originalValue,
                Guid newValue, NextFindChanges next)
            {
                return _results;
            }
        }
    }
}