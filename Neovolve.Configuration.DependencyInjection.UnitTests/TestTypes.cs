namespace Neovolve.Configuration.DependencyInjection.UnitTests
{
    using System.Diagnostics.CodeAnalysis;

    internal class EmptyClass
    {
    }

    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
    internal class GetExceptionType
    {
        private string _second = Guid.NewGuid().ToString();
        public string First { get; set; } = Guid.NewGuid().ToString();

        public string Second { get => throw new InvalidOperationException(); set => _second = value; }

        public string Third { get; set; } = Guid.NewGuid().ToString();
    }

    internal class InheritedType : SimpleType
    {
        public Guid Other { get; set; }
    }

    internal class NestedRecords
    {
        public ChildRecord First { get; set; }
        public ChildRecord Second { get; set; }
    }

    internal class NestedType
    {
        public InheritedType Child { get; set; }
        public int Something { get; set; }
    }

    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
    internal class PrivateSetType : ISimpleType
    {
        public string? First { get; set; } = Guid.NewGuid().ToString();
        public string? Second { get; private set; } = Guid.NewGuid().ToString();
        public string? Third { get; set; } = Guid.NewGuid().ToString();
    }

    internal interface ISimpleType
    {
        string? First { get; }
        string? Second { get; }
        string? Third { get; }
    }

    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
    internal class ReadOnlyType : ISimpleType
    {
        public string? First { get; set; } = Guid.NewGuid().ToString();
        public string? Second { get; } = Guid.NewGuid().ToString();
        public string? Third { get; set; } = Guid.NewGuid().ToString();
    }

    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
    internal class SetExceptionType
    {
        private readonly string _second = Guid.NewGuid().ToString();
        public string First { get; set; } = Guid.NewGuid().ToString();

        public string Second { get => _second; set => throw new InvalidOperationException(); }

        public string Third { get; set; } = Guid.NewGuid().ToString();
    }

    internal class SimpleType : ISimpleType
    {
        public string? First { get; set; }
        public string? Second { get; set; }
        public string? Third { get; set; }
    }

    internal record ChildRecord(string Name, int Start, bool End);
}