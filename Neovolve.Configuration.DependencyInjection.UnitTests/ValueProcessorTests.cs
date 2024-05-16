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
        public void FindChangesReturnsFindChangesWhenNoEvaluatorReturnsDefinitiveResult()
        {
            var propertyPath = Guid.NewGuid().ToString();
            var originalValue = "someValue";
            var newValue = "someValue";

            var firstEvaluator = Substitute.For<IChangeEvaluator>();
            var secondEvaluator = Substitute.For<IChangeEvaluator>();
            var thirdEvaluator = Substitute.For<IChangeEvaluator>();

            firstEvaluator.FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<NextFindChanges>()).Returns(x =>
            {
                var next = x.Arg<NextFindChanges>();

                return next(x.ArgAt<string>(0), x.ArgAt<object?>(1), x.ArgAt<object?>(2));
            });
            secondEvaluator.FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<NextFindChanges>()).Returns(x =>
            {
                var next = x.Arg<NextFindChanges>();

                return next(x.ArgAt<string>(0), x.ArgAt<object?>(1), x.ArgAt<object?>(2));
            });
            thirdEvaluator.FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<NextFindChanges>()).Returns(x =>
            {
                var next = x.Arg<NextFindChanges>();

                return next(x.ArgAt<string>(0), x.ArgAt<object?>(1), x.ArgAt<object?>(2));
            });

            var sut = new ValueProcessor(new List<IChangeEvaluator>
                { firstEvaluator, secondEvaluator, thirdEvaluator });

            var actual = sut.FindChanges(propertyPath, originalValue, newValue).ToList();

            actual.Should().BeEmpty();
            firstEvaluator.Received().FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<NextFindChanges>());
            secondEvaluator.Received().FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<NextFindChanges>());
            thirdEvaluator.Received().FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<NextFindChanges>());
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
                Arg.Any<NextFindChanges>()).Returns(x =>
            {
                var next = x.Arg<NextFindChanges>();

                return next(x.ArgAt<string>(0), x.ArgAt<object?>(1), x.ArgAt<object?>(2));
            });
            secondEvaluator.FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<NextFindChanges>()).Returns(x =>
            {
                var next = x.Arg<NextFindChanges>();

                return next(x.ArgAt<string>(0), x.ArgAt<object?>(1), x.ArgAt<object?>(2));
            });
            thirdEvaluator.FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<NextFindChanges>()).Returns(x =>
            {
                var next = x.Arg<NextFindChanges>();

                return next(x.ArgAt<string>(0), x.ArgAt<object?>(1), x.ArgAt<object?>(2));
            });
            lastEvaluator.FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<NextFindChanges>()).Returns(result);

            var sut = new ValueProcessor(new List<IChangeEvaluator>
                { firstEvaluator, secondEvaluator, thirdEvaluator, lastEvaluator });

            var data = sut.FindChanges(propertyPath, originalValue, newValue);

            var actual = data.ToList();

            actual.Should().BeEquivalentTo(result);
            firstEvaluator.Received().FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<NextFindChanges>());
            secondEvaluator.Received().FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<NextFindChanges>());
            thirdEvaluator.Received().FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<NextFindChanges>());
            lastEvaluator.Received().FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<NextFindChanges>());
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
                Arg.Any<NextFindChanges>()).Returns(x =>
            {
                var next = x.Arg<NextFindChanges>();

                return next(x.ArgAt<string>(0), x.ArgAt<object?>(1), x.ArgAt<object?>(2));
            });
            secondEvaluator.FindChanges(propertyPath, originalValue, newValue,
                    Arg.Any<NextFindChanges>())
                .Returns(_ => result);
            thirdEvaluator.FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<NextFindChanges>()).Returns(x =>
            {
                var next = x.Arg<NextFindChanges>();

                return next(x.ArgAt<string>(0), x.ArgAt<object?>(1), x.ArgAt<object?>(2));
            });
            lastEvaluator.FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<NextFindChanges>()).Returns(result);

            var sut = new ValueProcessor(new List<IChangeEvaluator>
                { firstEvaluator, secondEvaluator, thirdEvaluator, lastEvaluator });

            var actual = sut.FindChanges(propertyPath, originalValue, newValue).ToList();

            actual.Should().BeEquivalentTo(result);
            firstEvaluator.Received().FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<NextFindChanges>());
            secondEvaluator.Received().FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<NextFindChanges>());
            thirdEvaluator.DidNotReceive().FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<NextFindChanges>());
            lastEvaluator.DidNotReceive().FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<NextFindChanges>());
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
                Arg.Any<NextFindChanges>()).Returns(result);

            var sut = new ValueProcessor(new List<IChangeEvaluator> { evaluator });

            var actual = sut.FindChanges(propertyPath, originalValue, newValue).ToList();

            actual.Should().BeEquivalentTo(result);
        }

        [Fact]
        public void FindChangesReturnsResultsFromMultipleEvaluators()
        {
            var propertyPath = Guid.NewGuid().ToString();
            var originalValue = "someValue";
            var newValue = "someValue";
            var firstResult = new IdentifiedChange(propertyPath, Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            var secondResult = new IdentifiedChange(propertyPath, Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            var thirdResult = new IdentifiedChange(propertyPath, Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

            var firstEvaluator = Substitute.For<IChangeEvaluator>();
            var secondEvaluator = Substitute.For<IChangeEvaluator>();
            var thirdEvaluator = Substitute.For<IChangeEvaluator>();

            firstEvaluator.FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<NextFindChanges>()).Returns<IEnumerable<IdentifiedChange>>(x =>
            {
                var next = x.Arg<NextFindChanges>();

                var nextResults = next(x.ArgAt<string>(0), x.ArgAt<object?>(1), x.ArgAt<object?>(2));
                var data = new List<IdentifiedChange> { firstResult };
                data.AddRange(nextResults);

                return data;
            });
            secondEvaluator.FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<NextFindChanges>()).Returns(x =>
            {
                var next = x.Arg<NextFindChanges>();

                var nextResults = next(x.ArgAt<string>(0), x.ArgAt<object?>(1), x.ArgAt<object?>(2));
                var data = new List<IdentifiedChange> { secondResult };
                data.AddRange(nextResults);

                return data;
            });
            thirdEvaluator.FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<NextFindChanges>()).Returns(x =>
            {
                var next = x.Arg<NextFindChanges>();

                var nextResults = next(x.ArgAt<string>(0), x.ArgAt<object?>(1), x.ArgAt<object?>(2));
                var data = new List<IdentifiedChange> { thirdResult };
                data.AddRange(nextResults);

                return data;
            });

            var sut = new ValueProcessor(new List<IChangeEvaluator>
                { firstEvaluator, secondEvaluator, thirdEvaluator });

            var actual = sut.FindChanges(propertyPath, originalValue, newValue).ToList();

            actual.Should().HaveCount(3);
            actual.Should().Contain(firstResult);
            actual.Should().Contain(secondResult);
            actual.Should().Contain(thirdResult);

            firstEvaluator.Received().FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<NextFindChanges>());
            secondEvaluator.Received().FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<NextFindChanges>());
            thirdEvaluator.Received().FindChanges(propertyPath, originalValue, newValue,
                Arg.Any<NextFindChanges>());
        }
    }
}