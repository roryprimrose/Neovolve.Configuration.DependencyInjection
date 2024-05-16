namespace Neovolve.Configuration.DependencyInjection.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using Neovolve.Configuration.DependencyInjection.Comparison;
    using NSubstitute;

    public class ValueProcessorTests
    {
        [Fact]
        public void AreEqualReturnsAreEqualWhenNoEvaluatorReturnsDefinitiveResult()
        {
            var propertyPath = Guid.NewGuid().ToString();
            var originalValue = "someValue";
            var newValue = "someValue";

            var originalValueEvaluator = Substitute.For<IChangeEvaluator>();
            var newValueEvaluator = Substitute.For<IChangeEvaluator>();
            var thirdEvaluator = Substitute.For<IChangeEvaluator>();

            originalValueEvaluator.FindChanges(propertyPath, originalValue, newValue, Arg.Any<NextFindChanges>()).Returns(x =>
            {
                var next = x.Arg<NextFindChanges>();

                return next(x.Arg<string>(), x.ArgAt<object?>(0), x.ArgAt<object?>(1));
            });
            newValueEvaluator.FindChanges(propertyPath, originalValue, newValue, Arg.Any<NextFindChanges>()).Returns(x =>
            {
                var next = x.Arg<NextFindChanges>();

                return next(x.Arg<string>(), x.ArgAt<object?>(0), x.ArgAt<object?>(1));
            });
            thirdEvaluator.FindChanges(propertyPath, originalValue, newValue, Arg.Any<NextFindChanges>()).Returns(x =>
            {
                var next = x.Arg<NextFindChanges>();

                return next(x.Arg<string>(), x.ArgAt<object?>(0), x.ArgAt<object?>(1));
            });

            var sut = new ValueProcessor(new List<IChangeEvaluator>
                { originalValueEvaluator, newValueEvaluator, thirdEvaluator });

            var actual = sut.FindChanges(propertyPath, originalValue, newValue).ToList();
            
            actual.Should().BeEmpty();
            originalValueEvaluator.Received().FindChanges(propertyPath, originalValue, newValue, Arg.Any<NextFindChanges>());
            newValueEvaluator.Received().FindChanges(propertyPath, originalValue, newValue, Arg.Any<NextFindChanges>());
            thirdEvaluator.Received().FindChanges(propertyPath, originalValue, newValue, Arg.Any<NextFindChanges>());
        }

        [Fact]
        public void AreEqualReturnsAreEqualWhenNoEvaluatorsAreProvided()
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
        public void AreEqualReturnsResultFromLastEvaluatorWhenPriorEvaluatorsCallNext(bool expectsEqual)
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
                result = new() {new(propertyPath, "originalValue log", "newValue log")};
            }

            var originalValueEvaluator = Substitute.For<IChangeEvaluator>();
            var newValueEvaluator = Substitute.For<IChangeEvaluator>();
            var thirdEvaluator = Substitute.For<IChangeEvaluator>();
            var lastEvaluator = Substitute.For<IChangeEvaluator>();

            originalValueEvaluator.FindChanges(propertyPath, originalValue, newValue, Arg.Any<NextFindChanges>()).Returns(x =>
            {
                var next = x.Arg<NextFindChanges>();

                return next(x.Arg<string>(), x.ArgAt<object?>(0), x.ArgAt<object?>(1));
            });
            newValueEvaluator.FindChanges(propertyPath, originalValue, newValue, Arg.Any<NextFindChanges>()).Returns(x =>
            {
                var next = x.Arg<NextFindChanges>();

                return next(x.Arg<string>(), x.ArgAt<object?>(0), x.ArgAt<object?>(1));
            });
            thirdEvaluator.FindChanges(propertyPath, originalValue, newValue, Arg.Any<NextFindChanges>()).Returns(x =>
            {
                var next = x.Arg<NextFindChanges>();

                return next(x.Arg<string>(), x.ArgAt<object?>(0), x.ArgAt<object?>(1));
            });
            lastEvaluator.FindChanges(propertyPath, originalValue, newValue, Arg.Any<NextFindChanges>()).Returns(result);

            var sut = new ValueProcessor(new List<IChangeEvaluator>
                { originalValueEvaluator, newValueEvaluator, thirdEvaluator, lastEvaluator });

            var actual = sut.FindChanges(propertyPath, originalValue, newValue).ToList();

            actual.Should().BeEquivalentTo(result);
            originalValueEvaluator.Received().FindChanges(propertyPath, originalValue, newValue, Arg.Any<NextFindChanges>());
            newValueEvaluator.Received().FindChanges(propertyPath, originalValue, newValue, Arg.Any<NextFindChanges>());
            thirdEvaluator.Received().FindChanges(propertyPath, originalValue, newValue, Arg.Any<NextFindChanges>());
            lastEvaluator.Received().FindChanges(propertyPath, originalValue, newValue, Arg.Any<NextFindChanges>());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AreEqualReturnsResultFromOriginalValueEvaluatorNotCallingNext(bool expectsEqual)
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
                result = new() {new(propertyPath, "originalValue log", "newValue log")};
            }

            var originalValueEvaluator = Substitute.For<IChangeEvaluator>();
            var newValueEvaluator = Substitute.For<IChangeEvaluator>();
            var thirdEvaluator = Substitute.For<IChangeEvaluator>();
            var lastEvaluator = Substitute.For<IChangeEvaluator>();

            originalValueEvaluator.FindChanges(propertyPath, originalValue, newValue, Arg.Any<NextFindChanges>()).Returns(x =>
            {
                var next = x.Arg<NextFindChanges>();

                return next(x.Arg<string>(), x.ArgAt<object?>(0), x.ArgAt<object?>(1));
            });
            newValueEvaluator.FindChanges(propertyPath, originalValue, newValue, Arg.Any<NextFindChanges>())
                .Returns(_ => result);
            thirdEvaluator.FindChanges(propertyPath, originalValue, newValue, Arg.Any<NextFindChanges>()).Returns(x =>
            {
                var next = x.Arg<NextFindChanges>();

                return next(x.Arg<string>(), x.ArgAt<object?>(0), x.ArgAt<object?>(1));
            });
            lastEvaluator.FindChanges(propertyPath, originalValue, newValue, Arg.Any<NextFindChanges>()).Returns(result);

            var sut = new ValueProcessor(new List<IChangeEvaluator>
                { originalValueEvaluator, newValueEvaluator, thirdEvaluator, lastEvaluator });

            var actual = sut.FindChanges(propertyPath, originalValue, newValue).ToList();
            
            actual.Should().BeEquivalentTo(result);
            originalValueEvaluator.Received().FindChanges(propertyPath, originalValue, newValue, Arg.Any<NextFindChanges>());
            newValueEvaluator.Received().FindChanges(propertyPath, originalValue, newValue, Arg.Any<NextFindChanges>());
            thirdEvaluator.DidNotReceive().FindChanges(propertyPath, originalValue, newValue, Arg.Any<NextFindChanges>());
            lastEvaluator.DidNotReceive().FindChanges(propertyPath, originalValue, newValue, Arg.Any<NextFindChanges>());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AreEqualReturnsResultOfSingleEvaluatorWhenResultDetermined(bool expectsEqual)
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
                result = new() {new(propertyPath, "originalValue log", "newValue log")};
            }

            var evaluator = Substitute.For<IChangeEvaluator>();

            evaluator.FindChanges(propertyPath, originalValue, newValue, Arg.Any<NextFindChanges>()).Returns(result);

            var sut = new ValueProcessor(new List<IChangeEvaluator> { evaluator });

            var actual = sut.FindChanges(propertyPath, originalValue, newValue).ToList();
            
            actual.Should().BeEquivalentTo(result);
        }
    }
}