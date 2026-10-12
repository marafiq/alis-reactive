using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Alis.Reactive.Analyzers.NativeActionLink
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class NativeActionLinkPageFormAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ALIS002";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            title: "A NativeActionLink's request sees no page form",
            messageFormat: "A NativeActionLink carries a plan of its own, so its request cannot validate a form or send Form(): the link sees no page inputs.",
            category: "Alis.Reactive",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "A NativeActionLink's click sends one request to its href (the request type allows exactly one). Its plan is its own and sees no page inputs or forms.",
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
                if (actionLinkExtType == null)
                    return;

                var types = new CachedTypes(
                    actionLinkExtType,
                    compilationCtx.Compilation.GetTypeByMetadataName("Alis.Reactive.Builders.Requests.GatherBuilder`2"),
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

        // Validate and Form() reach for the page's form, which the link's own plan does not have.
        private static bool HasProhibitedPattern(
            LambdaExpressionSyntax lambda,
            SemanticModel semanticModel,
            CachedTypes types,
            System.Threading.CancellationToken cancellationToken)
        {
            foreach (var invocation in lambda.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                if (semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol is not IMethodSymbol methodSymbol)
                    continue;

                var containingType = methodSymbol.ContainingType.OriginalDefinition;
                var reachesThePageForm =
                    (methodSymbol.Name == "Form" && SymbolEqualityComparer.Default.Equals(containingType, types.GatherBuilderType))
                    || (methodSymbol.Name == "Validate" && SymbolEqualityComparer.Default.Equals(containingType, types.HttpRequestBuilderType));
                if (reachesThePageForm) return true;
            }

            return false;
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
            public readonly INamedTypeSymbol? GatherBuilderType;
            public readonly INamedTypeSymbol? HttpRequestBuilderType;

            public CachedTypes(
                INamedTypeSymbol actionLinkExtType,
                INamedTypeSymbol? gatherBuilderType,
                INamedTypeSymbol? httpRequestBuilderType)
            {
                ActionLinkExtType = actionLinkExtType;
                GatherBuilderType = gatherBuilderType;
                HttpRequestBuilderType = httpRequestBuilderType;
            }
        }
    }
}
