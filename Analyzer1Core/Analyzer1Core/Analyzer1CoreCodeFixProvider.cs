using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Semantics;
using Microsoft.CodeAnalysis.Editing;

namespace Analyzer1Core
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Analyzer1CoreCodeFixProvider)), Shared]
    public class Analyzer1CoreCodeFixProvider : CodeFixProvider
    {
        private const string title = "Improve perf by using Array.Empty";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(Analyzer1CoreAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
 
            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => UseArrayEmpty(context.Document, diagnostic, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private async Task<Document> UseArrayEmpty(Document document, Diagnostic diagnostic, CancellationToken c) {

            var root = await document.GetSyntaxRootAsync(c);
            var creationExpr = root.FindNode(diagnostic.Location.SourceSpan);


            var r = Array.Empty<string>();

            var semanticModel = await document.GetSemanticModelAsync(c);
            var operation = (IArrayCreationExpression)semanticModel.GetOperation(creationExpr);
            var generic = operation.Type;

            var generator = SyntaxGenerator.GetGenerator(document);
            var arrayType = semanticModel.Compilation.GetTypeByMetadataName("System.Array");
            var typeExpr = generator.TypeExpression(arrayType);
            var genericName = generator.GenericName("Empty", generic);
            var memberAccess = generator.MemberAccessExpression(typeExpr, genericName);
            var invocationExpr = generator.InvocationExpression(memberAccess);

            var newRoot = root.ReplaceNode(creationExpr, invocationExpr);
            var newDocument = document.WithSyntaxRoot(newRoot);

            return newDocument;
        }
    }
}
