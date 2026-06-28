namespace Neovolve.Configuration.DependencyInjection.CodeFixes.UnitTests
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Xunit;

    public class NotHotReloadableCodeFixProviderTests
    {
        private static readonly DiagnosticDescriptor _descriptor = new(
            "NCDI001",
            "Configuration type cannot be hot reloaded",
            "Configuration type '{0}' cannot be hot reloaded",
            "Neovolve.Configuration",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        [Fact]
        public async Task ConvertsStructDeclarationToClass()
        {
            const string source = @"
namespace Sample
{
    public struct Settings
    {
        public string Name { get; set; }
    }
}";

            var fixedText = await ApplyFix(source, "Settings");

            fixedText.Should().Contain("public class Settings");
            fixedText.Should().NotContain("public struct Settings");
        }

        [Fact]
        public async Task ConvertsRecordStructDeclarationToRecordClass()
        {
            const string source = @"
namespace Sample
{
    public record struct Settings(string Name);
}";

            var fixedText = await ApplyFix(source, "Settings");

            fixedText.Should().Contain("public record class Settings(string Name)");
            fixedText.Should().NotContain("record struct");
        }

        [Fact]
        public async Task DoesNotOfferFixForReferenceTypeDeclaration()
        {
            const string source = @"
namespace Sample
{
    public sealed record Settings(string Name);
}";

            var actions = await GetActions(source, "Settings");

            actions.Should().BeEmpty();
        }

        private static async Task<IReadOnlyList<CodeAction>> GetActions(string source, string typeName)
        {
            var workspace = new AdhocWorkspace();

            var project = workspace.CurrentSolution
                .AddProject("Test", "Test", LanguageNames.CSharp)
                .WithCompilationOptions(
                    new CSharpCompilationOptions(
                        OutputKind.DynamicallyLinkedLibrary,
                        nullableContextOptions: NullableContextOptions.Enable))
                .WithParseOptions(new CSharpParseOptions(LanguageVersion.Latest));

            var document = project.AddDocument("Test.cs", source);

            var root = await document.GetSyntaxRootAsync(CancellationToken.None);
            root.Should().NotBeNull();

            var declaration = root!.DescendantNodes()
                .OfType<TypeDeclarationSyntax>()
                .Single(node => node.Identifier.ValueText == typeName);

            var diagnostic = Diagnostic.Create(_descriptor, Location.Create(root.SyntaxTree, declaration.Span), typeName);

            var provider = new NotHotReloadableCodeFixProvider();
            var actions = new List<CodeAction>();

            var context = new CodeFixContext(
                document,
                diagnostic,
                (action, _) => actions.Add(action),
                CancellationToken.None);

            await provider.RegisterCodeFixesAsync(context);

            return actions;
        }

        private static async Task<string> ApplyFix(string source, string typeName)
        {
            var actions = await GetActions(source, typeName);

            actions.Should().NotBeEmpty();

            var operations = await actions[0].GetOperationsAsync(CancellationToken.None);
            var applyChanges = operations.OfType<ApplyChangesOperation>().Single();

            var changedDocument = applyChanges.ChangedSolution.Projects
                .Single()
                .Documents
                .Single();

            var text = await changedDocument.GetTextAsync();

            return text.ToString();
        }
    }
}
