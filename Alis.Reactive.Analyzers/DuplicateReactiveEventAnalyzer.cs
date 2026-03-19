using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Alis.Reactive.Analyzers
{
    /// <summary>
    /// Error when .Reactive() is called multiple times for the same event on the same builder chain.
    /// Each event should have ONE .Reactive() call containing all the logic for that event.
    ///
    /// Catches:
    ///   .Reactive(plan, evt => evt.Changed, ...).Reactive(plan, evt => evt.Changed, ...)
    ///   .Reactive(plan, evt => evt.Changed, ...).Reactive(plan, evt => evt.Focus, ...).Reactive(plan, evt => evt.Changed, ...)
    ///
    /// Does NOT flag:
    ///   Different builders each with their own .Reactive(evt.Click) — separate components
    ///   .Reactive(evt.Changed) and .Reactive(evt.Focus) on same builder — different events
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
            description: "Each component event should have exactly one .Reactive() call per builder chain. Multiple calls for the same event create redundant plan entries.");

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

            if (!IsRazorGeneratedFile(invocation.SyntaxTree))
                return;

            if (!IsReactiveCall(invocation))
                return;

            var eventName = ExtractEventName(invocation);
            if (eventName == null)
                return;

            // Walk the receiver chain backwards — collect all .Reactive() calls on this builder
            var seenEvents = new HashSet<string>();
            var current = GetReceiverInvocation(invocation);

            while (current != null)
            {
                if (IsReactiveCall(current))
                {
                    var innerEvent = ExtractEventName(current);
                    if (innerEvent != null)
                        seenEvents.Add(innerEvent);
                }
                current = GetReceiverInvocation(current);
            }

            // If the current event was already seen in an earlier .Reactive() on this chain, flag it
            if (seenEvents.Contains(eventName))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(Rule, invocation.GetLocation(), eventName));
            }
        }

        private static bool IsReactiveCall(InvocationExpressionSyntax invocation)
        {
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                return memberAccess.Name.Identifier.Text == "Reactive";
            return false;
        }

        private static InvocationExpressionSyntax? GetReceiverInvocation(InvocationExpressionSyntax invocation)
        {
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess
                && memberAccess.Expression is InvocationExpressionSyntax receiver)
                return receiver;
            return null;
        }

        private static string? ExtractEventName(InvocationExpressionSyntax invocation)
        {
            var args = invocation.ArgumentList.Arguments;
            if (args.Count < 2)
                return null;

            var selectorArg = args[1].Expression;
            if (selectorArg is SimpleLambdaExpressionSyntax lambda
                && lambda.Body is MemberAccessExpressionSyntax memberAccess)
            {
                return memberAccess.Name.Identifier.Text;
            }

            return null;
        }

        private static bool IsRazorGeneratedFile(SyntaxTree tree)
        {
            var path = tree.FilePath;
            if (string.IsNullOrEmpty(path)) return false;

            return path.EndsWith(".cshtml", System.StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".cshtml.g.cs", System.StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".g.cs", System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
