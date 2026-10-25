using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Alis.Reactive.Analyzers.ConditionalChain
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class IncompleteConditionalChainAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ALIS001";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            title: "Incomplete conditional chain",
            messageFormat: "Incomplete conditional chain — call .Then() to complete the condition",
            category: "Alis.Reactive",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "A conditional or guard chain was started but never completed with .Then(). " +
                         "The condition will not be included in the plan.",
            helpLinkUri: null);

        // The DSL's chains that are not yet in the plan: a source chosen without an operator, and a
        // condition built without .Then(), whether a guard or the all-guard and any-guard .And and .Or build.
        private static readonly ImmutableArray<string> IncompleteChainTypes = ImmutableArray.Create(
            "Alis.Reactive.Builders.Conditions.ConditionSourceBuilder`3",
            "Alis.Reactive.Builders.Conditions.GuardBuilder`1",
            "Alis.Reactive.Builders.Conditions.AllGuard`1",
            "Alis.Reactive.Builders.Conditions.AnyGuard`1");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(
                GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(compilationCtx =>
            {
                var incompleteChainTypes = IncompleteChainTypes
                    .Select(compilationCtx.Compilation.GetTypeByMetadataName)
                    .OfType<INamedTypeSymbol>()
                    .ToImmutableArray();

                if (incompleteChainTypes.IsEmpty)
                    return;

                compilationCtx.RegisterSyntaxNodeAction(
                    nodeCtx => AnalyzeExpressionStatement(nodeCtx, incompleteChainTypes),
                    SyntaxKind.ExpressionStatement);
            });
        }

        private static void AnalyzeExpressionStatement(
            SyntaxNodeAnalysisContext context,
            ImmutableArray<INamedTypeSymbol> incompleteChainTypes)
        {
            var statement = (ExpressionStatementSyntax)context.Node;
            if (!AnalyzerHelpers.IsRazorGeneratedFile(statement.SyntaxTree))
                return;

            var type = context.SemanticModel.GetTypeInfo(statement.Expression, context.CancellationToken).Type;
            if (type == null)
                return;

            if (incompleteChainTypes.Any(chainType => AnalyzerHelpers.IsClosedGenericOf(type, chainType)))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, statement.Expression.GetLocation()));
            }
        }
    }
}
