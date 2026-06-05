using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Alis.Reactive.Analyzers.Validation
{
    /// <summary>
    /// Reports FluentValidation <c>.When()</c> and <c>.Unless()</c> calls inside
    /// <c>ReactiveValidator&lt;T&gt;</c> because their arbitrary C# predicates cannot
    /// become client-executable validation conditions. Reactive Plan
    /// <c>PipelineBuilder.When()</c> conditions stay outside this diagnostic.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ServerOnlyConditionAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ALIS006";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            title: "FV condition is server-only in ReactiveValidator",
            messageFormat: "FV .{0}() is server-only in ReactiveValidator \u2014 use WhenField() for client-side conditional validation",
            category: "Alis.Reactive.Validation",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "FluentValidation's .When()/.Unless() conditions contain arbitrary C# predicates that cannot be " +
                         "serialized for client-side execution. Use ReactiveValidator.WhenField() instead \u2014 it constrains " +
                         "conditions to simple field comparisons (truthy, falsy, eq, neq) that the client runtime can evaluate.",
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
        /// Walks the receiver chain to find a <c>RuleFor</c> or <c>RuleForEach</c> call.
        /// This distinguishes FluentValidation <c>.When()</c> from Reactive Plan
        /// <c>PipelineBuilder.When()</c>, which has no rule-builder ancestor.
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
