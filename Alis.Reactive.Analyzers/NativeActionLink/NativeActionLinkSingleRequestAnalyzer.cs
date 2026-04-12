using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Alis.Reactive.Analyzers.NativeActionLink
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class NativeActionLinkSingleRequestAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ALIS002";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            title: "NativeActionLink must stay a single request chain",
            messageFormat: "NativeActionLink supports exactly one bounded request chain. Parallel, Chained, nested HTTP, IncludeAll, and validation are not allowed.",
            category: "Alis.Reactive",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "NativeActionLink is limited to one existing HTTP request chain serialized through data-reactive-* attributes.",
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
                var actionLinkExtType = compilationCtx.Compilation.GetTypeByMetadataName(
                    "Alis.Reactive.Native.Components.NativeActionLinkHtmlExtensions");
                var pipelineBuilderType = compilationCtx.Compilation.GetTypeByMetadataName(
                    "Alis.Reactive.Builders.PipelineBuilder`1");

                if (actionLinkExtType == null || pipelineBuilderType == null)
                    return;

                var types = new CachedTypes(
                    actionLinkExtType,
                    pipelineBuilderType,
                    compilationCtx.Compilation.GetTypeByMetadataName("Alis.Reactive.Builders.Requests.ResponseBuilder`1"),
                    compilationCtx.Compilation.GetTypeByMetadataName("Alis.Reactive.Builders.Requests.GatherBuilder`1"),
                    compilationCtx.Compilation.GetTypeByMetadataName("Alis.Reactive.Builders.Requests.HttpRequestBuilder`1"));

                compilationCtx.RegisterSyntaxNodeAction(
                    nodeCtx => AnalyzeInvocation(nodeCtx, types),
                    SyntaxKind.InvocationExpression);
            });
        }

        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context, CachedTypes types)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            if (!AnalyzerHelpers.IsRazorGeneratedFile(invocation.SyntaxTree))
                return;

            if (!IsNativeActionLinkInvocation(invocation, context.SemanticModel, types, context.CancellationToken))
                return;

            var lambda = invocation.ArgumentList.Arguments
                .Select(a => a.Expression)
                .OfType<LambdaExpressionSyntax>()
                .LastOrDefault();

            if (lambda == null)
                return;

            if (HasProhibitedPattern(lambda, context.SemanticModel, types, context.CancellationToken))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, lambda.GetLocation()));
            }
        }

        /// <summary>
        /// Scans the lambda body for patterns that are prohibited in NativeActionLink:
        /// Parallel, Chained, IncludeAll, Validate, or != 1 HTTP request start.
        /// </summary>
        private static bool HasProhibitedPattern(
            LambdaExpressionSyntax lambda,
            SemanticModel semanticModel,
            CachedTypes types,
            System.Threading.CancellationToken cancellationToken)
        {
            var requestStartCount = 0;

            foreach (var inv in lambda.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                if (semanticModel.GetSymbolInfo(inv, cancellationToken).Symbol is not IMethodSymbol sym)
                    continue;

                var containingType = sym.ContainingType.OriginalDefinition;

                if (IsProhibitedCall(sym.Name, containingType, types))
                    return true;

                if (AnalyzerHelpers.HttpMethodNames.Contains(sym.Name)
                    && SymbolEqualityComparer.Default.Equals(containingType, types.PipelineBuilderType))
                {
                    requestStartCount++;
                }
            }

            return requestStartCount != 1;
        }

        /// <summary>
        /// Returns true for Parallel, Chained, IncludeAll, or Validate calls
        /// on their respective builder types.
        /// </summary>
        private static bool IsProhibitedCall(
            string methodName, INamedTypeSymbol containingType, CachedTypes types)
        {
            switch (methodName)
            {
                case "Parallel":
                    return SymbolEqualityComparer.Default.Equals(containingType, types.PipelineBuilderType);
                case "Chained":
                    return types.ResponseBuilderType != null
                        && SymbolEqualityComparer.Default.Equals(containingType, types.ResponseBuilderType);
                case "IncludeAll":
                    return types.GatherBuilderType != null
                        && SymbolEqualityComparer.Default.Equals(containingType, types.GatherBuilderType);
                case "Validate":
                    return types.HttpRequestBuilderType != null
                        && SymbolEqualityComparer.Default.Equals(containingType, types.HttpRequestBuilderType);
                default:
                    return false;
            }
        }

        private static bool IsNativeActionLinkInvocation(
            InvocationExpressionSyntax invocation,
            SemanticModel semanticModel,
            CachedTypes types,
            System.Threading.CancellationToken cancellationToken)
        {
            if (semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol is not IMethodSymbol symbol)
                return false;

            return symbol.Name == "NativeActionLink"
                && SymbolEqualityComparer.Default.Equals(
                    symbol.ContainingType.OriginalDefinition, types.ActionLinkExtType);
        }

        private readonly struct CachedTypes
        {
            public readonly INamedTypeSymbol ActionLinkExtType;
            public readonly INamedTypeSymbol PipelineBuilderType;
            public readonly INamedTypeSymbol? ResponseBuilderType;
            public readonly INamedTypeSymbol? GatherBuilderType;
            public readonly INamedTypeSymbol? HttpRequestBuilderType;

            public CachedTypes(
                INamedTypeSymbol actionLinkExtType,
                INamedTypeSymbol pipelineBuilderType,
                INamedTypeSymbol? responseBuilderType,
                INamedTypeSymbol? gatherBuilderType,
                INamedTypeSymbol? httpRequestBuilderType)
            {
                ActionLinkExtType = actionLinkExtType;
                PipelineBuilderType = pipelineBuilderType;
                ResponseBuilderType = responseBuilderType;
                GatherBuilderType = gatherBuilderType;
                HttpRequestBuilderType = httpRequestBuilderType;
            }
        }
    }
}
