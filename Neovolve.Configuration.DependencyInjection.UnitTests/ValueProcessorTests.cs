namespace Neovolve.Configuration.DependencyInjection.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Neovolve.Configuration.DependencyInjection.Comparison;
    using NSubstitute;
    using Xunit.Abstractions;

    public class ValueProcessorTests
    {
        private readonly ITestOutputHelper _output;

        public ValueProcessorTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void FindChangesReturnsFindChangesWhenNoEvaluatorReturnsDefinitiveResult()
        {
            var propertyPath = Guid.NewGuid().ToString();
            var originalValue = "someValue";
            var newValue = "someValue";

            var firstEvaluator = Substitute.For<IChangeEvaluator>();
            var secondEvaluator = Substitute.For<IChangeEvaluator>();
            var thirdEvaluator = Substitute.For<IChangeEvaluator>();

            firstEvaluator.FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<Func<string, object?, object?, IEnumerable<IdentifiedChange>>>()).Returns(x =>
            {
                var next = x.Arg<Func<string, object?, object?, IEnumerable<IdentifiedChange>>>();

                return next(x.ArgAt<string>(0), x.ArgAt<object?>(1), x.ArgAt<object?>(2));
            });
            secondEvaluator.FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<Func<string, object?, object?, IEnumerable<IdentifiedChange>>>()).Returns(x =>
            {
                var next = x.Arg<Func<string, object?, object?, IEnumerable<IdentifiedChange>>>();
                
                return next(x.ArgAt<string>(0), x.ArgAt<object?>(1), x.ArgAt<object?>(2));
            });
            thirdEvaluator.FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<Func<string, object?, object?, IEnumerable<IdentifiedChange>>>()).Returns(x =>
            {
                var next = x.Arg<Func<string, object?, object?, IEnumerable<IdentifiedChange>>>();
                
                return next(x.ArgAt<string>(0), x.ArgAt<object?>(1), x.ArgAt<object?>(2));
            });

            var sut = new ValueProcessor(new List<IChangeEvaluator>
                { firstEvaluator, secondEvaluator, thirdEvaluator });

            var actual = sut.FindChanges(propertyPath, originalValue, newValue).ToList();

            actual.Should().BeEmpty();
            firstEvaluator.Received().FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<Func<string, object?, object?, IEnumerable<IdentifiedChange>>>());
            secondEvaluator.Received().FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<Func<string, object?, object?, IEnumerable<IdentifiedChange>>>());
            thirdEvaluator.Received().FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<Func<string, object?, object?, IEnumerable<IdentifiedChange>>>());
        }

        [Fact]
        public void FindChangesReturnsFindChangesWhenNoEvaluatorsAreProvided()
        {
            var propertyPath = Guid.NewGuid().ToString();
            var originalValue = "someValue";
            var newValue = "someValue";

            var sut = new ValueProcessor(Array.Empty<IChangeEvaluator>());

            var actual = sut.FindChanges(propertyPath, originalValue, newValue).ToList();

            actual.Should().BeEmpty();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void FindChangesReturnsResultFromLastEvaluatorWhenPriorEvaluatorsCallNext(bool expectsEqual)
        {
            var propertyPath = Guid.NewGuid().ToString();
            var originalValue = "someValue";
            var newValue = "someValue";
            List<IdentifiedChange> result;

            if (expectsEqual)
            {
                result = new();
            }
            else
            {
                result = new() { new(propertyPath, "originalValue log", "newValue log") };
            }

            var firstEvaluator = Substitute.For<IChangeEvaluator>();
            var secondEvaluator = Substitute.For<IChangeEvaluator>();
            var thirdEvaluator = Substitute.For<IChangeEvaluator>();
            var lastEvaluator = Substitute.For<IChangeEvaluator>();

            firstEvaluator.FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<Func<string, object?, object?, IEnumerable<IdentifiedChange>>>()).Returns(x =>
            {
                var next = x.Arg<Func<string, object?, object?, IEnumerable<IdentifiedChange>>>();
                
                return next(x.ArgAt<string>(0), x.ArgAt<object?>(1), x.ArgAt<object?>(2));
            });
            secondEvaluator.FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<Func<string, object?, object?, IEnumerable<IdentifiedChange>>>()).Returns(x =>
            {
                var next = x.Arg<Func<string, object?, object?, IEnumerable<IdentifiedChange>>>();
                
                return next(x.ArgAt<string>(0), x.ArgAt<object?>(1), x.ArgAt<object?>(2));
            });
            thirdEvaluator.FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<Func<string, object?, object?, IEnumerable<IdentifiedChange>>>()).Returns(x =>
            {
                var next = x.Arg<Func<string, object?, object?, IEnumerable<IdentifiedChange>>>();
                
                return next(x.ArgAt<string>(0), x.ArgAt<object?>(1), x.ArgAt<object?>(2));
            });
            lastEvaluator.FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<Func<string, object?, object?, IEnumerable<IdentifiedChange>>>()).Returns(result);

            var sut = new ValueProcessor(new List<IChangeEvaluator>
                { firstEvaluator, secondEvaluator, thirdEvaluator, lastEvaluator });

            var data = sut.FindChanges(propertyPath, originalValue, newValue);

            var actual = data.ToList();

            actual.Should().BeEquivalentTo(result);
            firstEvaluator.Received().FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<Func<string, object?, object?, IEnumerable<IdentifiedChange>>>());
            secondEvaluator.Received().FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<Func<string, object?, object?, IEnumerable<IdentifiedChange>>>());
            thirdEvaluator.Received().FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<Func<string, object?, object?, IEnumerable<IdentifiedChange>>>());
            lastEvaluator.Received().FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<Func<string, object?, object?, IEnumerable<IdentifiedChange>>>());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void FindChangesReturnsResultFromOriginalValueEvaluatorNotCallingNext(bool expectsEqual)
        {
            var propertyPath = Guid.NewGuid().ToString();
            var originalValue = "someValue";
            var newValue = "someValue";
            List<IdentifiedChange> result;

            if (expectsEqual)
            {
                result = new();
            }
            else
            {
                result = new() { new(propertyPath, "originalValue log", "newValue log") };
            }

            var firstEvaluator = Substitute.For<IChangeEvaluator>();
            var secondEvaluator = Substitute.For<IChangeEvaluator>();
            var thirdEvaluator = Substitute.For<IChangeEvaluator>();
            var lastEvaluator = Substitute.For<IChangeEvaluator>();

            firstEvaluator.FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<Func<string, object?, object?, IEnumerable<IdentifiedChange>>>()).Returns(x =>
            {
                var next = x.Arg<Func<string, object?, object?, IEnumerable<IdentifiedChange>>>();
                
                return next(x.ArgAt<string>(0), x.ArgAt<object?>(1), x.ArgAt<object?>(2));
            });
            secondEvaluator.FindChanges(propertyPath, originalValue, newValue,
                    Arg.Any<Func<string, object?, object?, IEnumerable<IdentifiedChange>>>())
                .Returns(_ => result);
            thirdEvaluator.FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<Func<string, object?, object?, IEnumerable<IdentifiedChange>>>()).Returns(x =>
            {
                var next = x.Arg<Func<string, object?, object?, IEnumerable<IdentifiedChange>>>();
                
                return next(x.ArgAt<string>(0), x.ArgAt<object?>(1), x.ArgAt<object?>(2));
            });
            lastEvaluator.FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<Func<string, object?, object?, IEnumerable<IdentifiedChange>>>()).Returns(result);

            var sut = new ValueProcessor(new List<IChangeEvaluator>
                { firstEvaluator, secondEvaluator, thirdEvaluator, lastEvaluator });

            var actual = sut.FindChanges(propertyPath, originalValue, newValue).ToList();

            actual.Should().BeEquivalentTo(result);
            firstEvaluator.Received().FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<Func<string, object?, object?, IEnumerable<IdentifiedChange>>>());
            secondEvaluator.Received().FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<Func<string, object?, object?, IEnumerable<IdentifiedChange>>>());
            thirdEvaluator.DidNotReceive().FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<Func<string, object?, object?, IEnumerable<IdentifiedChange>>>());
            lastEvaluator.DidNotReceive().FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<Func<string, object?, object?, IEnumerable<IdentifiedChange>>>());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void FindChangesReturnsResultOfSingleEvaluatorWhenResultDetermined(bool expectsEqual)
        {
            var propertyPath = Guid.NewGuid().ToString();
            var originalValue = "someValue";
            var newValue = "someValue";
            List<IdentifiedChange> result;

            if (expectsEqual)
            {
                result = new();
            }
            else
            {
                result = new() { new(propertyPath, "originalValue log", "newValue log") };
            }

            var evaluator = Substitute.For<IChangeEvaluator>();

            evaluator.FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<Func<string, object?, object?, IEnumerable<IdentifiedChange>>>()).Returns(result);

            var sut = new ValueProcessor(new List<IChangeEvaluator> { evaluator });

            var actual = sut.FindChanges(propertyPath, originalValue, newValue).ToList();

            actual.Should().BeEquivalentTo(result);
        }

        [Fact]
        public void FindChangesReturnsResultsFromMultipleEvaluators()
        {
            throw new NotImplementedException();
        }
    }
}