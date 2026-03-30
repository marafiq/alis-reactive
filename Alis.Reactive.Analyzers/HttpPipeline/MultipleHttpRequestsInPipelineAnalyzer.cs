using System;
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

        private static readonly ImmutableHashSet<string> HttpMethodNames =
            ImmutableHashSet.Create("Get", "Post", "Put", "Delete");

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

            if (!IsRazorGeneratedFile(lambda.SyntaxTree))
                return;

            // Only analyze lambdas whose parameter is PipelineBuilder<TModel>
            var pipelineParamName = GetPipelineParameterName(
                lambda, context.SemanticModel, pipelineBuilderType, context.CancellationToken);

            if (pipelineParamName == null)
                return;

            // Only block-bodied lambdas can have multiple statements
            if (!(lambda.Body is BlockSyntax block))
                return;

            // Count top-level expression statements that start an HTTP request chain on the pipeline parameter.
            // Do NOT descend into nested lambdas — those belong to Response, Parallel, WhileLoading, etc.
            var httpStatementCount = 0;
            foreach (var statement in block.Statements)
            {
                if (!(statement is ExpressionStatementSyntax exprStatement))
                    continue;

                if (IsHttpRequestChainOnParameter(exprStatement.Expression, pipelineParamName,
                    context.SemanticModel, pipelineBuilderType, context.CancellationToken))
                {
                    httpStatementCount++;
                    if (httpStatementCount >= 2)
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(Rule, statement.GetLocation()));
                    }
                }
            }
        }

        /// <summary>
        /// Walks a fluent invocation chain (a.B().C().D()) to find the innermost invocation
        /// that is a member access on the pipeline parameter, and checks if it is an HTTP method.
        /// </summary>
        private static bool IsHttpRequestChainOnParameter(
            ExpressionSyntax expression,
            string parameterName,
            SemanticModel semanticModel,
            INamedTypeSymbol pipelineBuilderType,
            System.Threading.CancellationToken cancellationToken)
        {
            // Unwrap the fluent chain: a.Post("/url").Response(r => ...) is
            // InvocationExpression(MemberAccess(InvocationExpression(MemberAccess(a, Post)), Response))
            // We need to find the root of this chain.
            var current = expression;

            while (current is InvocationExpressionSyntax invocation)
            {
                if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                {
                    // Check if this specific invocation is an HTTP method on the pipeline parameter
                    if (HttpMethodNames.Contains(memberAccess.Name.Identifier.Text))
                    {
                        // Verify the receiver is the pipeline parameter identifier
                        if (memberAccess.Expression is IdentifierNameSyntax identifier
                            && identifier.Identifier.Text == parameterName)
                        {
                            // Semantic check: confirm this method belongs to PipelineBuilder<TModel>
                            var symbol = semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol;
                            if (symbol is IMethodSymbol methodSymbol
                                && SymbolEqualityComparer.Default.Equals(
                                    methodSymbol.ContainingType.OriginalDefinition, pipelineBuilderType))
                            {
                                return true;
                            }
                        }
                    }

                    // Descend into the receiver to find deeper in the chain
                    current = memberAccess.Expression;
                }
                else
                {
                    break;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the name of the lambda parameter that is a PipelineBuilder, or null if none.
        /// </summary>
        private static string? GetPipelineParameterName(
            LambdaExpressionSyntax lambda,
            SemanticModel semanticModel,
            INamedTypeSymbol pipelineBuilderType,
            System.Threading.CancellationToken cancellationToken)
        {
            if (lambda is SimpleLambdaExpressionSyntax simple)
            {
                var symbol = semanticModel.GetDeclaredSymbol(simple.Parameter, cancellationToken);
                if (symbol != null && IsPipelineBuilderType(symbol.Type, pipelineBuilderType))
                    return simple.Parameter.Identifier.Text;
            }

            if (lambda is ParenthesizedLambdaExpressionSyntax parens)
            {
                foreach (var param in parens.ParameterList.Parameters)
                {
                    var symbol = semanticModel.GetDeclaredSymbol(param, cancellationToken);
                    if (symbol != null && IsPipelineBuilderType(symbol.Type, pipelineBuilderType))
                        return param.Identifier.Text;
                }
            }

            return null;
        }

        private static bool IsPipelineBuilderType(ITypeSymbol? type, INamedTypeSymbol pipelineBuilderType)
        {
            if (type is not INamedTypeSymbol named) return false;
            if (!named.IsGenericType) return false;
            return SymbolEqualityComparer.Default.Equals(named.ConstructedFrom, pipelineBuilderType);
        }

        private static bool IsRazorGeneratedFile(SyntaxTree tree)
        {
            var path = tree.FilePath;
            if (string.IsNullOrEmpty(path)) return false;

            return path.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".cshtml.g.cs", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase);
        }
    }
}
