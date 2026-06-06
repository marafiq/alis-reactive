using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Alis.Reactive.Analyzers.ReactiveEvent
{
    /// <summary>
    /// Error when .Reactive() is called multiple times for the same event on the same builder chain.
    /// Each event should have ONE .Reactive() call containing all the logic for that event.
    /// Different events and separate component builders are allowed.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DuplicateReactiveEventAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ALIS003";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            title: "Duplicate .Reactive() for the same event",
            messageFormat: "Multiple .Reactive() calls for '{0}' on the same builder chain. Combine into a single .Reactive() call.",
            category: "Alis.Reactive",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Each component event should have exactly one .Reactive() call per builder chain. Multiple calls for the same event create redundant Reactive Plan behaviors.",
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

            if (!IsReactiveCall(invocation))
                return;

            var eventName = ExtractEventName(invocation);
            if (eventName == null)
                return;

            if (HasEarlierReactiveCallForEvent(invocation, eventName))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(Rule, invocation.GetLocation(), eventName));
            }
        }

        private static bool HasEarlierReactiveCallForEvent(
            InvocationExpressionSyntax invocation, string eventName)
        {
            var current = AnalyzerHelpers.GetReceiverInvocation(invocation);
            while (current != null)
            {
                if (IsReactiveCall(current))
                {
                    var innerEvent = ExtractEventName(current);
                    if (innerEvent == eventName)
                        return true;
                }
                current = AnalyzerHelpers.GetReceiverInvocation(current);
            }
            return false;
        }

        private static bool IsReactiveCall(InvocationExpressionSyntax invocation)
        {
            return invocation.Expression is MemberAccessExpressionSyntax memberAccess
                && memberAccess.Name.Identifier.Text == "Reactive";
        }

        private static string? ExtractEventName(InvocationExpressionSyntax invocation)
        {
            var args = invocation.ArgumentList.Arguments;
            if (args.Count < 2)
                return null;

            if (args[1].Expression is SimpleLambdaExpressionSyntax lambda
                && lambda.Body is MemberAccessExpressionSyntax memberAccess)
            {
                return memberAccess.Name.Identifier.Text;
            }

            return null;
        }
    }
}
