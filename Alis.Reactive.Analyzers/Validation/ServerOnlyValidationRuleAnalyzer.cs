using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Alis.Reactive.Analyzers.Validation
{
    /// <summary>
    /// Info diagnostic when FluentValidation methods that produce server-only rules are used
    /// inside a <c>ReactiveValidator&lt;T&gt;</c>. These rules cannot be extracted for
    /// client-side validation and will silently drop during extraction.
    ///
    /// Detected methods: IsInEnum, Must, MustAsync, Custom, CustomAsync.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ServerOnlyValidationRuleAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ALIS005";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            title: "Server-only validation rule in ReactiveValidator",
            messageFormat: "'{0}' is server-only \u2014 not extractable for client-side validation in ReactiveValidator",
            category: "Alis.Reactive.Validation",
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "This FluentValidation rule type cannot be serialized to JSON for client-side execution. " +
                         "It will only run during server-side validation. If you need client-side validation, use " +
                         "supported rules (NotEmpty, MinLength, MaxLength, EmailAddress, Matches, InclusiveBetween, " +
                         "GreaterThan, LessThan, Equal, NotEqual, CreditCard).",
            helpLinkUri: "");

        private static readonly ImmutableHashSet<string> ServerOnlyMethods =
            ImmutableHashSet.Create(StringComparer.Ordinal,
                "IsInEnum",
                "Must",
                "MustAsync",
                "Custom",
                "CustomAsync"
            );

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

            if (!ServerOnlyMethods.Contains(methodName))
                return;

            var classDecl = FindContainingClass(invocation);
            if (classDecl == null)
                return;

            if (!ExtendsReactiveValidator(classDecl))
                return;

            context.ReportDiagnostic(
                Diagnostic.Create(Rule, memberAccess.Name.GetLocation(), methodName));
        }

        private static ClassDeclarationSyntax? FindContainingClass(SyntaxNode node)
        {
            var current = node.Parent;
            while (current != null)
            {
                if (current is ClassDeclarationSyntax classDecl)
                    return classDecl;
                current = current.Parent;
            }
            return null;
        }

        private static bool ExtendsReactiveValidator(ClassDeclarationSyntax classDecl)
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
