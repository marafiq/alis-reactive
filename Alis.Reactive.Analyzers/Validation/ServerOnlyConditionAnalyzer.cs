using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Alis.Reactive.Analyzers.Validation
{
    /// <summary>
    /// Reports FluentValidation <c>.When()</c> and <c>.Unless()</c> calls inside
    /// <c>ReactiveValidator&lt;T&gt;</c> because arbitrary C# predicates cannot become
    /// client-executable validation conditions.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ServerOnlyConditionAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ALIS006";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            title: "A rule's own condition is server-only in ReactiveValidator",
            messageFormat: "A rule's own .{0}() is server-only in ReactiveValidator \u2014 wrap the rules in When(x => ..., () => ...) for a browser condition",
            category: "Alis.Reactive.Validation",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A .When()/.Unless() chained on a rule is a compiled C# predicate, so the rules it conditions stay on " +
                         "the server. ReactiveValidator records the lambda of a When(x => ..., () => ...) block instead, and the " +
                         "browser applies the same condition when it compares model members with fixed values.",
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
                var reactiveValidatorType = compilationCtx.Compilation.GetTypeByMetadataName(
                    "Alis.Reactive.FluentValidator.ReactiveValidator`1");

                if (reactiveValidatorType == null)
                    return;

                compilationCtx.RegisterSyntaxNodeAction(
                    nodeCtx => AnalyzeInvocation(nodeCtx, reactiveValidatorType),
                    SyntaxKind.InvocationExpression);
            });
        }

        private static void AnalyzeInvocation(
            SyntaxNodeAnalysisContext context,
            INamedTypeSymbol reactiveValidatorType)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            if (!(invocation.Expression is MemberAccessExpressionSyntax memberAccess))
                return;

            var methodName = memberAccess.Name.Identifier.Text;
            if (methodName != "When" && methodName != "Unless")
                return;

            if (!IsOnRuleForChain(memberAccess.Expression))
                return;

            var classDecl = AnalyzerHelpers.FindContainingClass(invocation);
            if (classDecl == null)
                return;

            if (!AnalyzerHelpers.InheritsFromReactiveValidator(
                classDecl, context.SemanticModel, reactiveValidatorType, context.CancellationToken))
                return;

            context.ReportDiagnostic(
                Diagnostic.Create(Rule, memberAccess.Name.GetLocation(), methodName));
        }

        /// <summary>
        /// Finds a rule-builder ancestor so Reactive Plan <c>PipelineBuilder.When()</c>
        /// stays outside this FluentValidation diagnostic.
        /// </summary>
        private static bool IsOnRuleForChain(ExpressionSyntax expression)
        {
            var current = expression;

            while (current is InvocationExpressionSyntax invocation)
            {
                if (invocation.Expression is MemberAccessExpressionSyntax access)
                {
                    var name = access.Name.Identifier.Text;
                    if (name == "RuleFor" || name == "RuleForEach")
                        return true;
                    current = access.Expression;
                    continue;
                }

                if (invocation.Expression is IdentifierNameSyntax identifier)
                {
                    var name = identifier.Identifier.Text;
                    return name == "RuleFor" || name == "RuleForEach";
                }

                break;
            }

            return false;
        }
    }
}
