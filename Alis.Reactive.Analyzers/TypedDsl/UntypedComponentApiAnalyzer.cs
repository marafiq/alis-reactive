using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Alis.Reactive.Analyzers.TypedDsl
{
    /// <summary>
    /// Blocks untyped public API from creeping into any component: a shipped Fusion/Native slice, or
    /// one an application brings. A component is a declaration (a <c>Component</c> or <c>JsObject</c>
    /// subtype) and its render helpers (methods that return <c>RenderedComponent&lt;,&gt;</c> or extend
    /// <c>InputBoundField&lt;,&gt;</c>), wherever they live. The DSL is typed by design: developers never
    /// author a raw <c>object</c>, a member/method selector string, or a plan wire type. Plugin name is
    /// the only sanctioned string, and it lives at the plugin boundary, never in a component.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UntypedComponentApiAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ALIS009";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            title: "Component public API must be typed",
            messageFormat: "Public component API '{0}' exposes untyped '{1}'. Onboard a typed DSL (typed source/value/enum) and keep wire types in private overloads; developers never author raw object, member strings, or plan wire types.",
            category: "Alis.Reactive",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Components, shipped or brought, expose typed DSL only. A bare object, a member/method/action selector string, or a plan wire type in a public component signature lets JS-style untyped access creep into views.",
            helpLinkUri: null);

        // string parameters named like API selectors are stringly APIs, not typed values.
        private static readonly ImmutableHashSet<string> SelectorParamNames =
            ImmutableHashSet.Create(
                StringComparer.Ordinal,
                "action", "method", "methodName", "member", "memberName",
                "property", "propertyName", "kind", "op", "eventName");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
        }

        private static void AnalyzeMethod(SymbolAnalysisContext context)
        {
            var method = (IMethodSymbol)context.Symbol;

            if (method.DeclaredAccessibility != Accessibility.Public)
                return;
            if (method.MethodKind != MethodKind.Ordinary)
                return;
            if (!IsComponentApi(method))
                return;
            if (HasTypedDslExemption(method))
                return;

            if (IsUntypedValue(method.ReturnType))
                Report(context, method, method.Locations, Describe(method.ReturnType));

            foreach (var parameter in method.Parameters)
            {
                if (IsUntypedValue(parameter.Type))
                    Report(context, method, parameter.Locations, Describe(parameter.Type));
                else if (IsStringSelector(parameter))
                    Report(context, method, parameter.Locations, "string " + parameter.Name);
            }
        }

        // A small, greppable, reason-bearing set of sanctioned exceptions (e.g. a bridge to a
        // Syncfusion MVC builder slot typed as object). Keeps the gate hard for everything else.
        private static bool HasTypedDslExemption(IMethodSymbol method)
        {
            foreach (var attribute in method.GetAttributes())
            {
                if (attribute.AttributeClass?.ToDisplayString()
                    == "Alis.Reactive.Components.TypedDslExemptionAttribute")
                    return true;
            }

            return false;
        }

        // A component's public surface, wherever it is declared: the shipped slices' namespaces, a
        // declaration class, and the render helpers that put a component on a page.
        private static bool IsComponentApi(IMethodSymbol method)
        {
            var containingType = method.ContainingType;
            var ns = containingType.ContainingNamespace?.ToDisplayString();
            var inShippedSlice = ns != null
                && (ns.StartsWith("Alis.Reactive.Fusion.Components", StringComparison.Ordinal)
                    || ns.StartsWith("Alis.Reactive.Native.Components", StringComparison.Ordinal));
            return inShippedSlice
                || DerivesFrom(containingType, "Alis.Reactive.JsObject")
                || IsNamed(method.ReturnType, "Alis.Reactive.RenderedComponent<TComponent, TModel>")
                || (method.IsExtensionMethod
                    && IsNamed(method.Parameters[0].Type, "Alis.Reactive.InputBoundField<TModel, TProp>"));
        }

        private static bool DerivesFrom(INamedTypeSymbol type, string baseName)
        {
            for (var current = type.BaseType; current != null; current = current.BaseType)
            {
                if (current.ToDisplayString() == baseName) return true;
            }

            return false;
        }

        private static bool IsNamed(ITypeSymbol type, string definitionName) =>
            type is INamedTypeSymbol named && named.OriginalDefinition.ToDisplayString() == definitionName;

        // Bare object / object? — the untyped value that "lets things slip".
        // Accurate by construction: Expression&lt;Func&lt;T, object?&gt;&gt; selectors are NOT flagged because the
        // parameter type is Expression&lt;&gt;, not System.Object; object[] is reported via the array branch.
        private static bool IsUntypedValue(ITypeSymbol type)
        {
            if (type.SpecialType == SpecialType.System_Object)
                return true;
            return type is IArrayTypeSymbol array && array.ElementType.SpecialType == SpecialType.System_Object;
        }

        private static bool IsStringSelector(IParameterSymbol parameter) =>
            parameter.Type.SpecialType == SpecialType.System_String
            && SelectorParamNames.Contains(parameter.Name);

        private static string Describe(ITypeSymbol type) => type.ToDisplayString();

        private static void Report(
            SymbolAnalysisContext context,
            IMethodSymbol method,
            ImmutableArray<Location> locations,
            string offending)
        {
            var location = locations.Length > 0 ? locations[0] : method.Locations[0];
            context.ReportDiagnostic(Diagnostic.Create(Rule, location, method.Name, offending));
        }
    }
}
