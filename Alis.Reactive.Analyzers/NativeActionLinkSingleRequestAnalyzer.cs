using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Alis.Reactive.Analyzers
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
            description: "NativeActionLink is limited to one existing HTTP request chain serialized through data-reactive-* attributes.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(
                GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, Microsoft.CodeAnalysis.CSharp.SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            if (!IsRazorGeneratedFile(invocation.SyntaxTree))
                return;

            if (!IsNativeActionLinkInvocation(invocation, context.SemanticModel, context.CancellationToken))
                return;

            var lambda = invocation.ArgumentList.Arguments
                .Select(a => a.Expression)
                .OfType<LambdaExpressionSyntax>()
                .LastOrDefault();

            if (lambda == null)
                return;

            var descendantInvocations = lambda.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .ToArray();

            if (descendantInvocations.Any(i => IsParallelInvocation(i, context.SemanticModel, context.CancellationToken))
                || descendantInvocations.Any(i => IsChainedInvocation(i, context.SemanticModel, context.CancellationToken))
                || descendantInvocations.Any(i => IsIncludeAllInvocation(i, context.SemanticModel, context.CancellationToken))
                || descendantInvocations.Any(i => IsValidationInvocation(i, context.SemanticModel, context.CancellationToken)))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, lambda.GetLocation()));
                return;
            }

            var requestStartCount = descendantInvocations.Count(i =>
                IsPipelineRequestStart(i, context.SemanticModel, context.CancellationToken));

            if (requestStartCount != 1)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, lambda.GetLocation()));
            }
        }

        private static bool IsNativeActionLinkInvocation(
            InvocationExpressionSyntax invocation,
            SemanticModel semanticModel,
            System.Threading.CancellationToken cancellationToken)
        {
            if (semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol is not IMethodSymbol symbol)
                return false;

            return symbol.Name == "NativeActionLink"
                && symbol.ContainingType.OriginalDefinition.ToDisplayString()
                    == "Alis.Reactive.Native.Components.NativeActionLinkHtmlExtensions";
        }

        private static bool IsPipelineRequestStart(
            InvocationExpressionSyntax invocation,
            SemanticModel semanticModel,
            System.Threading.CancellationToken cancellationToken)
        {
            if (semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol is not IMethodSymbol symbol)
                return false;

            if (!(symbol.Name == "Get" || symbol.Name == "Post" || symbol.Name == "Put" || symbol.Name == "Delete"))
                return false;

            return symbol.ContainingType.OriginalDefinition.ToDisplayString()
                == "Alis.Reactive.Builders.PipelineBuilder<TModel>";
        }

        private static bool IsParallelInvocation(
            InvocationExpressionSyntax invocation,
            SemanticModel semanticModel,
            System.Threading.CancellationToken cancellationToken)
        {
            if (semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol is not IMethodSymbol symbol)
                return false;

            return symbol.Name == "Parallel"
                && symbol.ContainingType.OriginalDefinition.ToDisplayString()
                    == "Alis.Reactive.Builders.PipelineBuilder<TModel>";
        }

        private static bool IsChainedInvocation(
            InvocationExpressionSyntax invocation,
            SemanticModel semanticModel,
            System.Threading.CancellationToken cancellationToken)
        {
            if (semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol is not IMethodSymbol symbol)
                return false;

            return symbol.Name == "Chained"
                && symbol.ContainingType.OriginalDefinition.ToDisplayString()
                    == "Alis.Reactive.Builders.Requests.ResponseBuilder<TModel>";
        }

        private static bool IsIncludeAllInvocation(
            InvocationExpressionSyntax invocation,
            SemanticModel semanticModel,
            System.Threading.CancellationToken cancellationToken)
        {
            if (semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol is not IMethodSymbol symbol)
                return false;

            return symbol.Name == "IncludeAll"
                && symbol.ContainingType.OriginalDefinition.ToDisplayString()
                    == "Alis.Reactive.Builders.Requests.GatherBuilder<TModel>";
        }

        private static bool IsValidationInvocation(
            InvocationExpressionSyntax invocation,
            SemanticModel semanticModel,
            System.Threading.CancellationToken cancellationToken)
        {
            if (semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol is not IMethodSymbol symbol)
                return false;

            return symbol.Name == "Validate"
                && symbol.ContainingType.OriginalDefinition.ToDisplayString()
                    == "Alis.Reactive.Builders.Requests.HttpRequestBuilder<TModel>";
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
