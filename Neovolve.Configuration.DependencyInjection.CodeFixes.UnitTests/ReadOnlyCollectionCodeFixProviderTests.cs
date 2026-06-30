namespace Neovolve.Configuration.DependencyInjection.CodeFixes.UnitTests
{
    using System.Collections.Generic;
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

    public class ReadOnlyCollectionCodeFixProviderTests
    {
        private const string AddSetterTitle = "Add a setter to support hot reload";

        private const string SkipPropertyTitle = "Exclude the property from hot reload with [SkipConfigProperty]";

        private static readonly DiagnosticDescriptor _descriptor = new(
            "NCDI002",
            "Read-only collection configuration property cannot be hot reloaded",
            "Configuration property '{0}' is a read-only collection",
            "Neovolve.Configuration",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        [Fact]
        public async Task OffersBothFixesForGetOnlyCollectionProperty()
        {
            const string source = @"
namespace Sample
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public sealed class RequiredDataConfig
    {
        public ICollection<string> RequiredData { get; } = new Collection<string>();
    }
}";

            var actions = await GetActions(source, "RequiredData");

            actions.Select(action => action.Title).Should().Contain(new[] { AddSetterTitle, SkipPropertyTitle });
        }

        [Fact]
        public async Task AddsSetterToGetOnlyProperty()
        {
            const string source = @"
namespace Sample
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public sealed class RequiredDataConfig
    {
        public ICollection<string> RequiredData { get; } = new Collection<string>();
    }
}";

            var fixedText = await ApplyFix(source, "RequiredData", AddSetterTitle);

            fixedText.Should().Contain("public ICollection<string> RequiredData { get; set; }");
        }

        [Fact]
        public async Task PreservesInitializerWhenAddingSetter()
        {
            const string source = @"
namespace Sample
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public sealed class RequiredDataConfig
    {
        public ICollection<string> RequiredData { get; } = new Collection<string>();
    }
}";

            var fixedText = await ApplyFix(source, "RequiredData", AddSetterTitle);

            fixedText.Should().Contain("= new Collection<string>();");
        }

        [Fact]
        public async Task AppliesSkipConfigPropertyAttribute()
        {
            const string source = @"
namespace Sample
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public sealed class RequiredDataConfig
    {
        public ICollection<string> RequiredData { get; } = new Collection<string>();
    }
}";

            var fixedText = await ApplyFix(source, "RequiredData", SkipPropertyTitle);

            fixedText.Should().Contain("[SkipConfigProperty]");
            fixedText.Should().Contain("using Neovolve.Configuration.DependencyInjection.Generated;");
            fixedText.Should().Contain("public ICollection<string> RequiredData { get; } = new Collection<string>();");
        }

        [Fact]
        public async Task AppliesSkipConfigPropertyAttributeToExpressionBodiedProperty()
        {
            const string source = @"
namespace Sample
{
    using System.Collections.Generic;

    public sealed class RequiredDataConfig
    {
        private readonly ICollection<string> _data = new List<string>();
        public ICollection<string> RequiredData => _data;
    }
}";

            var fixedText = await ApplyFix(source, "RequiredData", SkipPropertyTitle);

            fixedText.Should().Contain("[SkipConfigProperty]");
        }

        [Fact]
        public async Task DoesNotOfferAddSetterWhenSetterAlreadyPresent()
        {
            const string source = @"
namespace Sample
{
    using System.Collections.Generic;

    public sealed class RequiredDataConfig
    {
        public ICollection<string> RequiredData { get; set; } = new List<string>();
    }
}";

            var actions = await GetActions(source, "RequiredData");

            actions.Select(action => action.Title).Should().NotContain(AddSetterTitle);
        }

        [Fact]
        public async Task DoesNotOfferAddSetterForExpressionBodiedProperty()
        {
            const string source = @"
namespace Sample
{
    using System.Collections.Generic;

    public sealed class RequiredDataConfig
    {
        private readonly ICollection<string> _data = new List<string>();
        public ICollection<string> RequiredData => _data;
    }
}";

            var actions = await GetActions(source, "RequiredData");

            actions.Select(action => action.Title).Should().NotContain(AddSetterTitle);
        }

        private static async Task<IReadOnlyList<CodeAction>> GetActions(string source, string propertyName)
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
                .OfType<PropertyDeclarationSyntax>()
                .Single(node => node.Identifier.ValueText == propertyName);

            var diagnostic = Diagnostic.Create(_descriptor, Location.Create(root.SyntaxTree, declaration.Span),
                propertyName);

            var provider = new ReadOnlyCollectionCodeFixProvider();
            var actions = new List<CodeAction>();

            var context = new CodeFixContext(
                document,
                diagnostic,
                (action, _) => actions.Add(action),
                CancellationToken.None);

            await provider.RegisterCodeFixesAsync(context);

            return actions;
        }

        private static async Task<string> ApplyFix(string source, string propertyName, string title)
        {
            var actions = await GetActions(source, propertyName);

            var action = actions.Single(candidate => candidate.Title == title);

            var operations = await action.GetOperationsAsync(CancellationToken.None);
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
