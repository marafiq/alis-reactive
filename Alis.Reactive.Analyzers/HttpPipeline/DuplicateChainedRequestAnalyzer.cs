using System;
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
    ///
    /// Catches:
    ///   .Response(r => r.Chained(c => c.Post("/step-1")).Chained(c => c.Post("/step-2")))
    ///   .Response(r => r.Chained(...).OnSuccess(...).Chained(...))
    ///
    /// Does NOT flag:
    ///   Single .Chained() on a ResponseBuilder chain
    ///   .Chained() on separate ResponseBuilder chains (separate .Response() calls)
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
            helpLinkUri: "");

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

            if (!IsChainedCall(invocation))
                return;

            // Walk the receiver chain backwards — if another .Chained() exists earlier, flag this one
            var current = GetReceiverInvocation(invocation);

            while (current != null)
            {
                if (IsChainedCall(current))
                {
                    // Report at the ".Chained" method name location
                    var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;
                    context.ReportDiagnostic(
                        Diagnostic.Create(Rule, memberAccess.Name.GetLocation()));
                    return;
                }
                current = GetReceiverInvocation(current);
            }
        }

        private static bool IsChainedCall(InvocationExpressionSyntax invocation)
        {
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                return memberAccess.Name.Identifier.Text == "Chained";
            return false;
        }

        private static InvocationExpressionSyntax? GetReceiverInvocation(InvocationExpressionSyntax invocation)
        {
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess
                && memberAccess.Expression is InvocationExpressionSyntax receiver)
                return receiver;
            return null;
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
