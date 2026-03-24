using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Alis.Reactive.Analyzers
{
    /// <summary>
    /// Info diagnostic when FV methods that produce server-only rules are used inside
    /// a ReactiveValidator&lt;T&gt;. These rules cannot be extracted for client-side validation.
    ///
    /// Detected methods: IsInEnum, Must, MustAsync, Custom, CustomAsync, SetValidator.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ServerOnlyValidationRuleAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ALIS004";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            title: "Server-only validation rule in ReactiveValidator",
            messageFormat: "'{0}' is server-only — not extractable for client-side validation in ReactiveValidator",
            category: "Alis.Reactive.Validation",
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "This FluentValidation rule type cannot be serialized to JSON for client-side execution. " +
                         "It will only run during server-side validation. If you need client-side validation, use " +
                         "supported rules (NotEmpty, MinLength, MaxLength, EmailAddress, Matches, InclusiveBetween, " +
                         "GreaterThan, LessThan, Equal, NotEqual, CreditCard).");

        private static readonly string[] ServerOnlyMethods = new[]
        {
            "IsInEnum",
            "Must",
            "MustAsync",
            "Custom",
            "CustomAsync",
            "SetValidator"
        };

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

            // Must be a member access: something.MethodName(...)
            if (!(invocation.Expression is MemberAccessExpressionSyntax memberAccess))
                return;

            var methodName = memberAccess.Name.Identifier.Text;

            // Check if this is one of the server-only method names
            if (!IsServerOnlyMethod(methodName))
                return;

            // Walk up the syntax tree to find the containing class declaration
            var classDecl = FindContainingClass(invocation);
            if (classDecl == null)
                return;

            // Check if the class extends ReactiveValidator<T> (syntax check on base type name)
            if (!ExtendsReactiveValidator(classDecl))
                return;

            context.ReportDiagnostic(
                Diagnostic.Create(Rule, memberAccess.Name.GetLocation(), methodName));
        }

        private static bool IsServerOnlyMethod(string methodName)
        {
            for (int i = 0; i < ServerOnlyMethods.Length; i++)
            {
                if (string.Equals(ServerOnlyMethods[i], methodName, StringComparison.Ordinal))
                    return true;
            }
            return false;
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
                if (typeName.Contains("ReactiveValidator"))
                    return true;
            }

            return false;
        }
    }
}
