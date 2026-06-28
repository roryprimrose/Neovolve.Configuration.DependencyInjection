// These types are passed directly to DefaultConfigUpdater.UpdateConfig in unit tests rather than being
// reached through a ConfigureWith<T> root, so they are marked for accessor generation here. The generator
// walks each type's graph, so child types (for example the children of NestedType and NestedRecords) are
// covered automatically.

using Neovolve.Configuration.DependencyInjection.Generated;
using Neovolve.Configuration.DependencyInjection.UnitTests.Models;

[assembly: GenerateConfigAccessors(
    typeof(SimpleType),
    typeof(InheritedType),
    typeof(NestedType),
    typeof(NestedRecords),
    typeof(PrivateSetType),
    typeof(GetExceptionType),
    typeof(SetExceptionType),
    typeof(ReadOnlyType),
    typeof(EmptyClass),
    typeof(ReadOnlyType<SimpleType>),
    typeof(ReadOnlyType<string>),
    typeof(ReadOnlyType<int>),
    typeof(ValueOnlyType))]
