namespace Neovolve.Configuration.DependencyInjection.CodeFixes
{
    using System;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    ///     The <see cref="ReadOnlyCollectionCodeFixProvider" /> class offers fixes for a read only collection
    ///     configuration property that cannot be hot reloaded.
    /// </summary>
    /// <remarks>
    ///     The provider registers two fixes on the <c>NCDI002</c> diagnostic the source generator raises for a read only
    ///     mutable collection property. The first adds a public setter so the reloaded collection can be assigned during a
    ///     configuration hot reload; it is only offered when the reported member is a get-only auto-property (it has a
    ///     getter accessor and no setter or init accessor). The second applies the <c>[SkipConfigProperty]</c> attribute so
    ///     the property is excluded from the configuration graph and intentionally bound once at startup.
    /// </remarks>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ReadOnlyCollectionCodeFixProvider))]
    [Shared]
    public sealed class ReadOnlyCollectionCodeFixProvider : CodeFixProvider
    {
        private const string DiagnosticId = "NCDI002";

        private const string AddSetterTitle = "Add a setter to support hot reload";

        private const string SkipPropertyTitle = "Exclude the property from hot reload with [SkipConfigProperty]";

        private const string SkipConfigPropertyAttributeName = "SkipConfigProperty";

        private const string GeneratedNamespace = "Neovolve.Configuration.DependencyInjection.Generated";

        /// <inheritdoc />
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(DiagnosticId);

        /// <inheritdoc />
        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        /// <inheritdoc />
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            if (root == null)
            {
                return;
            }

            foreach (var diagnostic in context.Diagnostics)
            {
                var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
                var declaration = node.FirstAncestorOrSelf<PropertyDeclarationSyntax>();

                if (declaration == null)
                {
                    continue;
                }

                if (CanAddSetter(declaration))
                {
                    var addSetter = CodeAction.Create(
                        AddSetterTitle,
                        cancellationToken => AddSetterAsync(context.Document, root, declaration, cancellationToken),
                        equivalenceKey: AddSetterTitle);

                    context.RegisterCodeFix(addSetter, diagnostic);
                }

                var skipProperty = CodeAction.Create(
                    SkipPropertyTitle,
                    cancellationToken => ApplySkipConfigPropertyAsync(context.Document, root, declaration,
                        cancellationToken),
                    equivalenceKey: SkipPropertyTitle);

                context.RegisterCodeFix(skipProperty, diagnostic);
            }
        }

        private static bool CanAddSetter(PropertyDeclarationSyntax declaration)
        {
            if (declaration.AccessorList == null)
            {
                // An expression bodied property has no accessor list, so a setter cannot be added by this fix.
                return false;
            }

            var hasGetter = false;

            foreach (var accessor in declaration.AccessorList.Accessors)
            {
                if (accessor.IsKind(SyntaxKind.SetAccessorDeclaration)
                    || accessor.IsKind(SyntaxKind.InitAccessorDeclaration))
                {
                    // The property already has a setter or init accessor, so there is nothing to add.
                    return false;
                }

                if (accessor.IsKind(SyntaxKind.GetAccessorDeclaration))
                {
                    hasGetter = true;
                }
            }

            return hasGetter;
        }

        private static Task<Document> AddSetterAsync(
            Document document,
            SyntaxNode root,
            PropertyDeclarationSyntax declaration,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var setter = SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

            var accessors = declaration.AccessorList!.Accessors.Add(setter);
            var accessorList = declaration.AccessorList.WithAccessors(accessors);
            var replacement = declaration.WithAccessorList(accessorList);

            var newRoot = root.ReplaceNode(declaration, replacement);

            return Task.FromResult(document.WithSyntaxRoot(newRoot));
        }

        private static Task<Document> ApplySkipConfigPropertyAsync(
            Document document,
            SyntaxNode root,
            PropertyDeclarationSyntax declaration,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var leadingTrivia = declaration.GetLeadingTrivia();
            var indentation = leadingTrivia.LastOrDefault(trivia => trivia.IsKind(SyntaxKind.WhitespaceTrivia));

            var attribute = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(SkipConfigPropertyAttributeName));

            // The attribute keeps the property's original leading trivia (blank lines and indentation) and sits on its
            // own line, while the property is re-indented to the same column.
            var attributeList = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attribute))
                .WithLeadingTrivia(leadingTrivia)
                .WithTrailingTrivia(SyntaxFactory.EndOfLine(Environment.NewLine), indentation);

            var replacement = declaration
                .WithLeadingTrivia(indentation)
                .WithAttributeLists(declaration.AttributeLists.Insert(0, attributeList));

            var newRoot = root.ReplaceNode(declaration, replacement);

            if (newRoot is CompilationUnitSyntax compilationUnit)
            {
                newRoot = EnsureGeneratedNamespaceImported(compilationUnit);
            }

            return Task.FromResult(document.WithSyntaxRoot(newRoot));
        }

        private static CompilationUnitSyntax EnsureGeneratedNamespaceImported(CompilationUnitSyntax compilationUnit)
        {
            var alreadyImported = compilationUnit.Usings
                .Any(directive => directive.Name?.ToString() == GeneratedNamespace);

            if (alreadyImported)
            {
                return compilationUnit;
            }

            var usingDirective = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(GeneratedNamespace))
                .WithTrailingTrivia(SyntaxFactory.EndOfLine(Environment.NewLine));

            return compilationUnit.AddUsings(usingDirective);
        }
    }
}

