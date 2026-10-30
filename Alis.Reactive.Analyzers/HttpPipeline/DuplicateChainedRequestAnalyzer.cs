using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Alis.Reactive.Analyzers.HttpPipeline
{
    /// <summary>
    /// Reports a second <c>.Chained()</c> call in one fluent chain on a response, which has one follow-up
    /// request: the request builder refuses the second when the view renders, and this reports it as the view compiles.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DuplicateChainedRequestAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ALIS007";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            title: "Duplicate .Chained() call on ResponseBuilder",
            messageFormat: "Duplicate .Chained() call — a response has one Chained request; chain the next request from that one's own Response(r => r.Chained(..))",
            category: "Alis.Reactive.HttpPipeline",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "A response has one follow-up request. The request builder refuses a second .Chained() when the view renders; this reports one written in the same fluent chain as the view compiles.",
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
