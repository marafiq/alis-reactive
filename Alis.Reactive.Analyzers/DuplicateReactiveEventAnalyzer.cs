using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Alis.Reactive.Analyzers
{
    /// <summary>
    /// Warns when .Reactive() is chained multiple times on the same builder for the same event.
    /// Each event should have ONE .Reactive() call containing all the logic for that event.
    ///
    /// BAD:  .Reactive(plan, evt => evt.Changed, (args, p) => { ... })
    ///       .Reactive(plan, evt => evt.Changed, (args, p) => { ... })
    ///
    /// GOOD: .Reactive(plan, evt => evt.Changed, (args, p) => { /* all logic here */ })
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DuplicateReactiveEventAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ALIS003";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            title: "Duplicate .Reactive() for the same event",
            messageFormat: "Multiple .Reactive() calls for '{0}' on the same builder. Combine into a single .Reactive() call.",
            category: "Alis.Reactive",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Each component event should have exactly one .Reactive() call. Multiple calls create duplicate plan entries. Combine all logic into a single .Reactive() pipeline.");

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

            // Only check Razor-generated files
            if (!IsRazorGeneratedFile(invocation.SyntaxTree))
                return;

            // Is this a .Reactive() call?
            if (!IsReactiveCall(invocation))
                return;

            // Is the receiver (what .Reactive() is called on) ALSO a .Reactive() call?
            var receiver = GetReceiverInvocation(invocation);
            if (receiver == null || !IsReactiveCall(receiver))
                return;

            // Both are .Reactive() calls — check if they have the same event selector
            var outerEvent = ExtractEventName(invocation);
            var innerEvent = ExtractEventName(receiver);

            if (outerEvent != null && innerEvent != null && outerEvent == innerEvent)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(Rule, invocation.GetLocation(), outerEvent));
            }
        }

        /// <summary>
        /// Checks if an invocation is a .Reactive() call by method name.
        /// </summary>
        private static bool IsReactiveCall(InvocationExpressionSyntax invocation)
        {
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                return memberAccess.Name.Identifier.Text == "Reactive";
            return false;
        }

        /// <summary>
        /// Gets the invocation that .Reactive() is called on (the receiver).
        /// For: expr.Reactive(...), returns expr if it's also an InvocationExpression.
        /// </summary>
        private static InvocationExpressionSyntax? GetReceiverInvocation(InvocationExpressionSyntax invocation)
        {
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess
                && memberAccess.Expression is InvocationExpressionSyntax receiver)
                return receiver;
            return null;
        }

        /// <summary>
        /// Extracts the event name from the event selector lambda: evt => evt.Changed → "Changed"
        /// The event selector is the second argument: .Reactive(plan, evt => evt.Changed, ...)
        /// </summary>
        private static string? ExtractEventName(InvocationExpressionSyntax invocation)
        {
            var args = invocation.ArgumentList.Arguments;
            if (args.Count < 2)
                return null;

            // Second argument should be a lambda: evt => evt.Changed
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
