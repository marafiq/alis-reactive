using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Alis.Reactive.Analyzers
{
    /// <summary>
    /// Warning when FluentValidation's .When() or .Unless() is used inside a ReactiveValidator.
    /// These are server-only conditions (arbitrary C# lambdas that can't serialize to JSON).
    /// The developer should use WhenField() instead for client-side conditional validation.
    ///
    /// Catches:
    ///   RuleFor(x => x.Name).NotEmpty().When(x => x.IsActive)
    ///   RuleFor(x => x.Name).NotEmpty().MaxLength(100).Unless(x => x.IsAdmin)
    ///
    /// Does NOT flag:
    ///   p.When(args, a => a.Value).Eq("Custom") — framework's When() on PipelineBuilder (no RuleFor ancestor)
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ServerOnlyConditionAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ALIS005";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            title: "FV condition is server-only in ReactiveValidator",
            messageFormat: "FV .{0}() is server-only in ReactiveValidator — use WhenField() for client-side conditional validation",
            category: "Alis.Reactive.Validation",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "FluentValidation's .When()/.Unless() conditions contain arbitrary C# predicates that cannot be " +
                         "serialized for client-side execution. Use ReactiveValidator.WhenField() instead — it constrains " +
                         "conditions to simple field comparisons (truthy, falsy, eq, neq) that the client runtime can evaluate.");

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

            // Check if the invocation is .When(...) or .Unless(...)
            if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
                return;

            var methodName = memberAccess.Name.Identifier.Text;
            if (methodName != "When" && methodName != "Unless")
                return;

            // Walk the receiver chain to see if we're on a RuleFor(...) chain
            if (!IsOnRuleForChain(memberAccess.Expression))
                return;

            // Walk up the syntax tree to find the containing class declaration
            if (!IsInsideReactiveValidator(invocation))
                return;

            context.ReportDiagnostic(
                Diagnostic.Create(Rule, invocation.GetLocation(), methodName));
        }

        /// <summary>
        /// Walk the receiver chain (the expression before .When/.Unless) to see if
        /// any ancestor invocation is a RuleFor call. This distinguishes FV's .When()
        /// (on a RuleFor chain) from the framework's .When() (on PipelineBuilder).
        ///
        /// Handles deeply chained calls like:
        ///   RuleFor(x => x.Name).NotEmpty().MaxLength(100).When(x => ...)
        ///   ↑ RuleFor is the root of the chain
        /// </summary>
        private static bool IsOnRuleForChain(ExpressionSyntax expression)
        {
            var current = expression;

            while (current != null)
            {
                if (current is InvocationExpressionSyntax invocation)
                {
                    if (invocation.Expression is MemberAccessExpressionSyntax access)
                    {
                        if (access.Name.Identifier.Text == "RuleFor" || access.Name.Identifier.Text == "RuleForEach")
                            return true;

                        // Keep walking up: the receiver of this invocation
                        current = access.Expression;
                        continue;
                    }

                    // Could be a simple invocation like RuleFor(...) without member access
                    if (invocation.Expression is IdentifierNameSyntax identifier)
                    {
                        if (identifier.Identifier.Text == "RuleFor" || identifier.Identifier.Text == "RuleForEach")
                            return true;
                    }

                    break;
                }

                // If the current node is something else (e.g., an identifier), stop walking
                break;
            }

            return false;
        }

        /// <summary>
        /// Walk up the syntax tree to find the containing class declaration and check
        /// if it extends ReactiveValidator (by checking base type names in the syntax).
        /// </summary>
        private static bool IsInsideReactiveValidator(SyntaxNode node)
        {
            var current = node.Parent;

            while (current != null)
            {
                if (current is ClassDeclarationSyntax classDecl)
                {
                    return HasReactiveValidatorBase(classDecl);
                }

                current = current.Parent;
            }

            return false;
        }

        /// <summary>
        /// Check if the class declaration has a base type containing "ReactiveValidator".
        /// Matches: ReactiveValidator&lt;T&gt;, ReactiveValidator&lt;MyModel&gt;, etc.
        /// </summary>
        private static bool HasReactiveValidatorBase(ClassDeclarationSyntax classDecl)
        {
            if (classDecl.BaseList == null)
                return false;

            foreach (var baseType in classDecl.BaseList.Types)
            {
                var typeName = baseType.Type.ToString();
                if (typeName.Contains("ReactiveValidator"))
                    return true;
            }

            return false;
        }
    }
}
