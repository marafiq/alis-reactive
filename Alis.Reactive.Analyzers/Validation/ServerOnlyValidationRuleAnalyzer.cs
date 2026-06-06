using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Alis.Reactive.Analyzers.Validation
{
    /// <summary>
    /// Reports <c>ReactiveValidator&lt;T&gt;</c> rules that cannot be serialized for
    /// client-side validation.
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
            helpLinkUri: null);

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
            if (!ServerOnlyMethods.Contains(methodName))
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
    }
}
