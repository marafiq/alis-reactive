using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Alis.Reactive.Analyzers.HttpPipeline
{
    /// <summary>
    /// Error when .Chained() is called more than once on the same ResponseBuilder fluent chain.
    /// Only the last .Chained() survives — earlier ones are silently overwritten.
    /// Separate ResponseBuilder chains are independent.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DuplicateChainedRequestAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ALIS007";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            title: "Duplicate .Chained() call on ResponseBuilder",
            messageFormat: "Duplicate .Chained() call — only the last chained request survives. Use a single .Chained() with the final request.",
            category: "Alis.Reactive.HttpPipeline",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Each ResponseBuilder chain should have at most one .Chained() call. Multiple calls silently overwrite — only the last one takes effect.",
            helpLinkUri: null);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(
                GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            if (!AnalyzerHelpers.IsRazorGeneratedFile(invocation.SyntaxTree))
                return;

            if (!IsChainedCall(invocation))
                return;

            if (HasEarlierChainedCall(invocation))
            {
                var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;
                context.ReportDiagnostic(
                    Diagnostic.Create(Rule, memberAccess.Name.GetLocation()));
            }
        }

        private static bool HasEarlierChainedCall(InvocationExpressionSyntax invocation)
        {
            var current = AnalyzerHelpers.GetReceiverInvocation(invocation);
            while (current != null)
            {
                if (IsChainedCall(current))
                    return true;
                current = AnalyzerHelpers.GetReceiverInvocation(current);
            }
            return false;
        }

        private static bool IsChainedCall(InvocationExpressionSyntax invocation)
        {
            return invocation.Expression is MemberAccessExpressionSyntax memberAccess
                && memberAccess.Name.Identifier.Text == "Chained";
        }
    }
}
