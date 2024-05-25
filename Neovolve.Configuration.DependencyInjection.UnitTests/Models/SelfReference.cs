namespace Neovolve.Configuration.DependencyInjection.UnitTests.Models
{
    public class SelfReferenceRoot
    {
        public SelfReference Self { get; set; } = new();
    }

    public class SelfReference
    {
        public SelfReference? Child { get; set; }
        public Guid Id { get; set; }

        public SelfReference? Parent { get; set; }
    }
}