namespace Neovolve.Configuration.DependencyInjection.UnitTests.Models
{
    public class SkippedTypesRoot
    {
        public SkippedTypes Skipped { get; set; } = new();
    }

    public class SkippedTypes
    {
        public Guid Id { get; set; }

        public Type? TypeReference { get; set; }
    }
}