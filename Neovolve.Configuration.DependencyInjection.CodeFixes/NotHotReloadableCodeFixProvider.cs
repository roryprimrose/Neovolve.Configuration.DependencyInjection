namespace Neovolve.Configuration.DependencyInjection.CodeFixes
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    ///     The <see cref="NotHotReloadableCodeFixProvider" /> class
    ///     converts a value type configuration declaration into a reference type so that injected
    ///     instances can receive configuration updates via hot reload.
    /// </summary>
    /// <remarks>
    ///     The fix is offered on the <c>NCDI001</c> diagnostic the source generator raises when a
    ///     configuration type cannot be hot reloaded. It is only registered when the reported declaration
    ///     is a value type (a <c>struct</c> or <c>record struct</c>); the "no writable properties" variant
    ///     of the diagnostic on a reference type is left for the author to resolve by adding settable
    ///     properties, because there is no single safe rewrite for that case.
    /// </remarks>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NotHotReloadableCodeFixProvider))]
    [Shared]
    public sealed class NotHotReloadableCodeFixProvider : CodeFixProvider
    {
        private const string DiagnosticId = "NCDI001";

        private const string Title = "Convert to a class to support hot reload";

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
                var declaration = node.FirstAncestorOrSelf<TypeDeclarationSyntax>();

                if (IsValueTypeDeclaration(declaration) == false)
                {
                    continue;
                }

                var action = CodeAction.Create(
                    Title,
                    cancellationToken => ConvertToClassAsync(context.Document, root, declaration!, cancellationToken),
                    equivalenceKey: Title);

                context.RegisterCodeFix(action, diagnostic);
            }
        }

        private static bool IsValueTypeDeclaration(TypeDeclarationSyntax? declaration)
        {
            if (declaration is StructDeclarationSyntax)
            {
                return true;
            }

            if (declaration is RecordDeclarationSyntax record
                && record.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword))
            {
                return true;
            }

            return false;
        }

        private static Task<Document> ConvertToClassAsync(
            Document document,
            SyntaxNode root,
            TypeDeclarationSyntax declaration,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            TypeDeclarationSyntax replacement;

            if (declaration is RecordDeclarationSyntax record)
            {
                // record struct -> record (which is a reference type). Replacing the struct keyword with the
                // class keyword keeps the positional parameter list and members intact.
                replacement = record.WithClassOrStructKeyword(
                    SyntaxFactory.Token(SyntaxKind.ClassKeyword).WithTriviaFrom(record.ClassOrStructKeyword));
            }
            else
            {
                var structDeclaration = (StructDeclarationSyntax)declaration;

                replacement = SyntaxFactory.ClassDeclaration(structDeclaration.Identifier)
                    .WithAttributeLists(structDeclaration.AttributeLists)
                    .WithModifiers(structDeclaration.Modifiers)
                    .WithKeyword(SyntaxFactory.Token(SyntaxKind.ClassKeyword).WithTriviaFrom(structDeclaration.Keyword))
                    .WithTypeParameterList(structDeclaration.TypeParameterList)
                    .WithBaseList(structDeclaration.BaseList)
                    .WithConstraintClauses(structDeclaration.ConstraintClauses)
                    .WithOpenBraceToken(structDeclaration.OpenBraceToken)
                    .WithMembers(structDeclaration.Members)
                    .WithCloseBraceToken(structDeclaration.CloseBraceToken)
                    .WithSemicolonToken(structDeclaration.SemicolonToken);
            }

            replacement = replacement
                .WithLeadingTrivia(declaration.GetLeadingTrivia())
                .WithTrailingTrivia(declaration.GetTrailingTrivia());

            var newRoot = root.ReplaceNode(declaration, replacement);

            return Task.FromResult(document.WithSyntaxRoot(newRoot));
        }
    }
}
