using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Alis.Reactive.Analyzers.Validation
{
    /// <summary>
    /// Warning when FluentValidation's <c>.When()</c> or <c>.Unless()</c> is used inside a
    /// <c>ReactiveValidator&lt;T&gt;</c>. These are server-only conditions (arbitrary C#
    /// lambdas that cannot serialize to JSON). Use <c>WhenField()</c> instead.
    ///
    /// Catches:
    ///   <c>RuleFor(x =&gt; x.Name).NotEmpty().When(x =&gt; x.IsActive)</c>
    ///   <c>RuleFor(x =&gt; x.Name).NotEmpty().Unless(x =&gt; x.IsAdmin)</c>
    ///
    /// Does NOT flag:
    ///   <c>p.When(args, a =&gt; a.Value).Eq("Custom")</c> — framework's When() on PipelineBuilder.
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

            if (!(invocation.Expression is MemberAccessExpressionSyntax memberAccess))
                return;

            var methodName = memberAccess.Name.Identifier.Text;
            if (methodName != "When" && methodName != "Unless")
                return;

            if (!IsOnRuleForChain(memberAccess.Expression))
                return;

            if (!IsInsideReactiveValidator(invocation))
                return;

            context.ReportDiagnostic(
                Diagnostic.Create(Rule, invocation.GetLocation(), methodName));
        }

        /// <summary>
        /// Walk the receiver chain (the expression before .When/.Unless) to find a
        /// RuleFor or RuleForEach call. This distinguishes FV's .When() (on a RuleFor chain)
        /// from the framework's .When() (on PipelineBuilder, which has no RuleFor ancestor).
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
                        var name = access.Name.Identifier.Text;
                        if (name == "RuleFor" || name == "RuleForEach")
                            return true;

                        current = access.Expression;
                        continue;
                    }

                    if (invocation.Expression is IdentifierNameSyntax identifier)
                    {
                        var name = identifier.Identifier.Text;
                        if (name == "RuleFor" || name == "RuleForEach")
                            return true;
                    }

                    break;
                }

                break;
            }

            return false;
        }

        private static bool IsInsideReactiveValidator(SyntaxNode node)
        {
            var current = node.Parent;

            while (current != null)
            {
                if (current is ClassDeclarationSyntax classDecl)
                    return HasReactiveValidatorBase(classDecl);

                current = current.Parent;
            }

            return false;
        }

        private static bool HasReactiveValidatorBase(ClassDeclarationSyntax classDecl)
        {
            if (classDecl.BaseList == null)
                return false;

            foreach (var baseType in classDecl.BaseList.Types)
            {
                var typeName = baseType.Type.ToString();
                // Check for ReactiveValidator<...> pattern — base type starts with "ReactiveValidator<"
                // or equals "ReactiveValidator" (raw, non-generic, unlikely but safe)
                if (typeName.StartsWith("ReactiveValidator<", StringComparison.Ordinal) || typeName == "ReactiveValidator")
                    return true;
            }

            return false;
        }
    }
}
