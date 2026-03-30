using System;
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
                var actionLinkExtType = compilationCtx.Compilation.GetTypeByMetadataName(
                    "Alis.Reactive.Native.Components.NativeActionLinkHtmlExtensions");
                var pipelineBuilderType = compilationCtx.Compilation.GetTypeByMetadataName(
                    "Alis.Reactive.Builders.PipelineBuilder`1");
                var parallelBuilderType = compilationCtx.Compilation.GetTypeByMetadataName(
                    "Alis.Reactive.Builders.ParallelBuilder`1");
                var responseBuilderType = compilationCtx.Compilation.GetTypeByMetadataName(
                    "Alis.Reactive.Builders.Requests.ResponseBuilder`1");
                var gatherBuilderType = compilationCtx.Compilation.GetTypeByMetadataName(
                    "Alis.Reactive.Builders.Requests.GatherBuilder`1");
                var httpRequestBuilderType = compilationCtx.Compilation.GetTypeByMetadataName(
                    "Alis.Reactive.Builders.Requests.HttpRequestBuilder`1");

                if (actionLinkExtType == null || pipelineBuilderType == null)
                    return;

                var cachedTypes = new CachedTypes(
                    actionLinkExtType,
                    pipelineBuilderType,
                    parallelBuilderType,
                    responseBuilderType,
                    gatherBuilderType,
                    httpRequestBuilderType);

                compilationCtx.RegisterSyntaxNodeAction(
                    nodeCtx => AnalyzeInvocation(nodeCtx, cachedTypes),
                    SyntaxKind.InvocationExpression);
            });
        }

        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context, CachedTypes types)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            if (!IsRazorGeneratedFile(invocation.SyntaxTree))
                return;

            if (!IsNativeActionLinkInvocation(invocation, context.SemanticModel, types, context.CancellationToken))
                return;

            var lambda = invocation.ArgumentList.Arguments
                .Select(a => a.Expression)
                .OfType<LambdaExpressionSyntax>()
                .LastOrDefault();

            if (lambda == null)
                return;

            var hasParallel = false;
            var hasChained = false;
            var hasIncludeAll = false;
            var hasValidate = false;
            var requestStartCount = 0;

            foreach (var inv in lambda.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                if (context.SemanticModel.GetSymbolInfo(inv, context.CancellationToken).Symbol
                    is not IMethodSymbol sym)
                    continue;

                var containingType = sym.ContainingType.OriginalDefinition;

                if (sym.Name == "Parallel"
                    && types.ParallelBuilderType != null
                    && SymbolEqualityComparer.Default.Equals(containingType, types.PipelineBuilderType))
                {
                    hasParallel = true;
                    break;
                }

                if (sym.Name == "Chained"
                    && types.ResponseBuilderType != null
                    && SymbolEqualityComparer.Default.Equals(containingType, types.ResponseBuilderType))
                {
                    hasChained = true;
                    break;
                }

                if (sym.Name == "IncludeAll"
                    && types.GatherBuilderType != null
                    && SymbolEqualityComparer.Default.Equals(containingType, types.GatherBuilderType))
                {
                    hasIncludeAll = true;
                    break;
                }

                if (sym.Name == "Validate"
                    && types.HttpRequestBuilderType != null
                    && SymbolEqualityComparer.Default.Equals(containingType, types.HttpRequestBuilderType))
                {
                    hasValidate = true;
                    break;
                }

                if (HttpMethodNames.Contains(sym.Name)
                    && SymbolEqualityComparer.Default.Equals(containingType, types.PipelineBuilderType))
                {
                    requestStartCount++;
                }
            }

            if (hasParallel || hasChained || hasIncludeAll || hasValidate)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, lambda.GetLocation()));
                return;
            }

            if (requestStartCount != 1)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, lambda.GetLocation()));
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

        private static bool IsRazorGeneratedFile(SyntaxTree tree)
        {
            var path = tree.FilePath;
            if (string.IsNullOrEmpty(path)) return false;

            return path.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".cshtml.g.cs", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase);
        }

        private readonly struct CachedTypes
        {
            public readonly INamedTypeSymbol ActionLinkExtType;
            public readonly INamedTypeSymbol PipelineBuilderType;
            public readonly INamedTypeSymbol? ParallelBuilderType;
            public readonly INamedTypeSymbol? ResponseBuilderType;
            public readonly INamedTypeSymbol? GatherBuilderType;
            public readonly INamedTypeSymbol? HttpRequestBuilderType;

            public CachedTypes(
                INamedTypeSymbol actionLinkExtType,
                INamedTypeSymbol pipelineBuilderType,
                INamedTypeSymbol? parallelBuilderType,
                INamedTypeSymbol? responseBuilderType,
                INamedTypeSymbol? gatherBuilderType,
                INamedTypeSymbol? httpRequestBuilderType)
            {
                ActionLinkExtType = actionLinkExtType;
                PipelineBuilderType = pipelineBuilderType;
                ParallelBuilderType = parallelBuilderType;
                ResponseBuilderType = responseBuilderType;
                GatherBuilderType = gatherBuilderType;
                HttpRequestBuilderType = httpRequestBuilderType;
            }
        }
    }
}
