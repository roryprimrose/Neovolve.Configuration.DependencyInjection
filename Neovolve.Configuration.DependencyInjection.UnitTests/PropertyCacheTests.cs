namespace Neovolve.Configuration.DependencyInjection.UnitTests
{
    using FluentAssertions;

    public class PropertyCacheTests
    {
        [Fact]
        public void GetBindablePropertiesReturnsCachedProperties()
        {
            var firstActual = typeof(CacheableType).GetBindableProperties(true);
            var secondActual = typeof(CacheableType).GetBindableProperties(true);

            firstActual.Should().BeSameAs(secondActual);
        }

        [Fact]
        public void GetBindablePropertiesReturnsPublicGetPropertiesOnly()
        {
            var actual = typeof(TargetType).GetBindableProperties(false).ToList();

            actual.Should().HaveCount(4);
            actual.Should().Contain(x => x.Name == nameof(TargetType.ReadOnly));
            actual.Should().Contain(x => x.Name == nameof(TargetType.ReadWrite));
            actual.Should().Contain(x => x.Name == nameof(TargetType.InternalWrite));
            actual.Should().Contain(x => x.Name == nameof(TargetType.PrivateWrite));
        }

        private class CacheableType
        {
            public int Value { get; set; }
        }

        private class TargetType
        {
            public int InternalRead { internal get; set; }

            public int InternalWrite { get; internal set; }

            public int PrivateRead { private get; set; }

            public int PrivateWrite { get; private set; }

            public int ReadOnly { get; private set; }

            public int ReadWrite { get; set; }

            public int WriteOnly { set => ReadOnly = value; }
        }
    }
}