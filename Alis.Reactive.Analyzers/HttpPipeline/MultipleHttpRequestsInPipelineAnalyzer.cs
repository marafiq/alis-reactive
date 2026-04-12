using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Alis.Reactive.Analyzers.HttpPipeline
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class MultipleHttpRequestsInPipelineAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ALIS008";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            title: "Multiple HTTP requests in one pipeline",
            messageFormat: "Multiple HTTP requests in one pipeline — only the last request survives. Use Parallel(...) for concurrent requests or separate triggers for independent requests.",
            category: "Alis.Reactive.HttpPipeline",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "When a PipelineBuilder lambda contains two or more top-level HTTP request statements " +
                         "(Get/Post/Put/Delete), only the last one survives at runtime. " +
                         "Use Parallel(...) for concurrent requests or separate triggers for independent requests.",
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
                var pipelineBuilderType = compilationCtx.Compilation.GetTypeByMetadataName(
                    "Alis.Reactive.Builders.PipelineBuilder`1");

                if (pipelineBuilderType == null)
                    return;

                compilationCtx.RegisterSyntaxNodeAction(
                    nodeCtx => AnalyzeLambda(nodeCtx, pipelineBuilderType),
                    SyntaxKind.SimpleLambdaExpression,
                    SyntaxKind.ParenthesizedLambdaExpression);
            });
        }

        private static void AnalyzeLambda(
            SyntaxNodeAnalysisContext context,
            INamedTypeSymbol pipelineBuilderType)
        {
            var lambda = (LambdaExpressionSyntax)context.Node;

            if (!AnalyzerHelpers.IsRazorGeneratedFile(lambda.SyntaxTree))
                return;

            var pipelineParamName = GetPipelineParameterName(
                lambda, context.SemanticModel, pipelineBuilderType, context.CancellationToken);

            if (pipelineParamName == null)
                return;

            if (!(lambda.Body is BlockSyntax block))
                return;

            var httpStatementCount = 0;
            foreach (var statement in block.Statements)
            {
                if (!(statement is ExpressionStatementSyntax exprStatement))
                    continue;

                if (!StartsHttpChainOnParameter(exprStatement.Expression, pipelineParamName,
                    context.SemanticModel, pipelineBuilderType, context.CancellationToken))
                    continue;

                httpStatementCount++;
                if (httpStatementCount >= 2)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, statement.GetLocation()));
                }
            }
        }

        /// <summary>
        /// Unwraps a fluent chain (a.Post("/url").Response(r => ...)) to find the root
        /// invocation, then checks if it is an HTTP method call on the pipeline parameter.
        /// </summary>
        private static bool StartsHttpChainOnParameter(
            ExpressionSyntax expression,
            string parameterName,
            SemanticModel semanticModel,
            INamedTypeSymbol pipelineBuilderType,
            System.Threading.CancellationToken cancellationToken)
        {
            var current = expression;

            while (current is InvocationExpressionSyntax invocation
                && invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                if (AnalyzerHelpers.HttpMethodNames.Contains(memberAccess.Name.Identifier.Text)
                    && memberAccess.Expression is IdentifierNameSyntax identifier
                    && identifier.Identifier.Text == parameterName)
                {
                    var symbol = semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol;
                    return symbol is IMethodSymbol methodSymbol
                        && SymbolEqualityComparer.Default.Equals(
                            methodSymbol.ContainingType.OriginalDefinition, pipelineBuilderType);
                }

                current = memberAccess.Expression;
            }

            return false;
        }

        private static string? GetPipelineParameterName(
            LambdaExpressionSyntax lambda,
            SemanticModel semanticModel,
            INamedTypeSymbol pipelineBuilderType,
            System.Threading.CancellationToken cancellationToken)
        {
            if (lambda is SimpleLambdaExpressionSyntax simple)
            {
                var symbol = semanticModel.GetDeclaredSymbol(simple.Parameter, cancellationToken);
                if (symbol != null && AnalyzerHelpers.IsClosedGenericOf(symbol.Type, pipelineBuilderType))
                    return simple.Parameter.Identifier.Text;
            }

            if (lambda is ParenthesizedLambdaExpressionSyntax parens)
            {
                foreach (var param in parens.ParameterList.Parameters)
                {
                    var symbol = semanticModel.GetDeclaredSymbol(param, cancellationToken);
                    if (symbol != null && AnalyzerHelpers.IsClosedGenericOf(symbol.Type, pipelineBuilderType))
                        return param.Identifier.Text;
                }
            }

            return null;
        }
    }
}
