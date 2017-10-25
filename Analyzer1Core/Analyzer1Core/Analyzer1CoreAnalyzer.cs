using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;

namespace Analyzer1Core
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class Analyzer1CoreAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "Analyzer1Core";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Performance";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, 
                                                                            Title, 
                                                                            MessageFormat, 
                                                                            Category, 
                                                                            DiagnosticSeverity.Warning, 
                                                                            isEnabledByDefault: true, 
                                                                            description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context) => context.RegisterCompilationStartAction(startContext => {
            var hasEmpty = startContext.Compilation.GetTypeByMetadataName("System.Array")?.GetMembers("Empty").Any();
            if (hasEmpty == true) {
                startContext.RegisterOperationAction(AnalyzeOperation,OperationKind.ArrayCreationExpression);
            }
        });

        private void AnalyzeOperation(OperationAnalysisContext context) {
          var creationExpression = (IArrayCreationExpression)context.Operation;

            if(creationExpression.DimensionSizes.Length == 1 && creationExpression.DimensionSizes[0].ConstantValue.HasValue) {
                object arrayDimension = creationExpression.DimensionSizes[0].ConstantValue.Value;
                if (arrayDimension is int && (int)arrayDimension == 0) {
                    var diagnostic = Diagnostic.Create(Rule, context.Operation.Syntax.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                }
            }

        }
    }
}
