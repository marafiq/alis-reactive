using System.Collections.Immutable;
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

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(
                GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(compilationCtx =>
            {
                var guardBuilderType = compilationCtx.Compilation.GetTypeByMetadataName(
                    "Alis.Reactive.Builders.Conditions.GuardBuilder`1");
                var conditionSourceBuilderType = compilationCtx.Compilation.GetTypeByMetadataName(
                    "Alis.Reactive.Builders.Conditions.ConditionSourceBuilder`2");

                if (guardBuilderType == null && conditionSourceBuilderType == null)
                    return;

                compilationCtx.RegisterSyntaxNodeAction(
                    nodeCtx => AnalyzeExpressionStatement(nodeCtx, guardBuilderType, conditionSourceBuilderType),
                    SyntaxKind.ExpressionStatement);
            });
        }

        private static void AnalyzeExpressionStatement(
            SyntaxNodeAnalysisContext context,
            INamedTypeSymbol? guardBuilderType,
            INamedTypeSymbol? conditionSourceBuilderType)
        {
            var statement = (ExpressionStatementSyntax)context.Node;
            if (!AnalyzerHelpers.IsRazorGeneratedFile(statement.SyntaxTree))
                return;

            var type = context.SemanticModel.GetTypeInfo(statement.Expression, context.CancellationToken).Type;
            if (type == null)
                return;

            if (AnalyzerHelpers.IsClosedGenericOf(type, guardBuilderType)
                || AnalyzerHelpers.IsClosedGenericOf(type, conditionSourceBuilderType))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, statement.Expression.GetLocation()));
            }
        }
    }
}
