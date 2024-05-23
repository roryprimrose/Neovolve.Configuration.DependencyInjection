namespace Neovolve.Configuration.DependencyInjection.UnitTests.Models
{
    public class IgnoredTypesRoot
    {
        public IgnoredTypes Ignored { get; set; } = new();
    }

    public class IgnoredTypes
    {
        public Guid Id { get; set; }

        public Type TypeReference { get; set; }
    }
}