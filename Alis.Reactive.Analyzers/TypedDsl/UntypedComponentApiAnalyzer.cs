using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Alis.Reactive.Analyzers.TypedDsl
{
    /// <summary>
    /// The public API is typed. On the whole exposed surface of the shipped packages, and on every
    /// component declaration and render helper wherever it is written, a public or protected member
    /// takes and returns typed values: no <c>object</c>, no <c>System.Type</c>, no untyped <c>Arg</c> on a
    /// public member, and no string that names a member, field, event, method or path. Strings stay for
    /// what is text: a message, a label, a URL, an element id the developer chose, a header name, a CSS
    /// class, and a JavaScript name at the vendor or plugin boundary, which <c>name:</c> carries.
    /// A deliberate exception names its reason with <c>[TypedDslExemption]</c>.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UntypedComponentApiAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ALIS009";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            title: "Public API must be typed",
            messageFormat: "Public API '{0}' exposes untyped '{1}'. Give it a typed form (a generic value, an expression, an enum, a declaration) and keep wire types internal; developers never author raw object, System.Type, member-naming strings or plan wire types.",
            category: "Alis.Reactive",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "The shipped packages and every component declaration expose typed DSL only. A bare object or System.Type, an untyped Arg on a public member, or a member-naming string in an exposed signature lets JS-style untyped access creep into views.",
            helpLinkUri: null);

        // The packages whose whole exposed surface is the developer's API.
        private static readonly ImmutableHashSet<string> ShippedAssemblies =
            ImmutableHashSet.Create(
                StringComparer.Ordinal,
                "Alis.Reactive",
                "Alis.Reactive.Fusion",
                "Alis.Reactive.Native",
                "Alis.Reactive.FluentValidator",
                "Alis.Reactive.DesignSystem",
                "Alis.Reactive.NativeTagHelpers");

        // A string parameter with one of these names names a member, not a value.
        private static readonly ImmutableHashSet<string> MemberNamingParameters =
            ImmutableHashSet.Create(
                StringComparer.Ordinal,
                "action", "method", "methodName", "member", "memberName",
                "property", "propertyName", "kind", "op", "eventName", "eventType",
                "field", "fieldName", "peer", "collection", "owner", "path", "propertyPath",
                "memberPath", "selector", "objectPath", "subscribe", "unsubscribe");

        private const string ExemptionAttribute = "Alis.Reactive.Components.TypedDslExemptionAttribute";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
            context.RegisterSymbolAction(AnalyzeProperty, SymbolKind.Property);
        }

        private static void AnalyzeMethod(SymbolAnalysisContext context)
        {
            var method = (IMethodSymbol)context.Symbol;
            var isAuthored = method.MethodKind == MethodKind.Ordinary || method.MethodKind == MethodKind.Constructor;
            if (!isAuthored || !IsExposed(method) || !InScope(method) || HasExemption(method))
                return;
            if (InheritsShapeFromOutside(method))
                return;

            if (method.MethodKind == MethodKind.Ordinary && IsUntypedValue(method.ReturnType))
                Report(context, method, method.Locations, Describe(method.ReturnType));

            foreach (var parameter in method.Parameters)
            {
                if (IsUntypedValue(parameter.Type))
                    Report(context, method, parameter.Locations, Describe(parameter.Type));
                else if (IsMemberNamingString(parameter))
                    Report(context, method, parameter.Locations, "string " + parameter.Name);
                else if (IsUntypedArgument(parameter.Type) && method.DeclaredAccessibility == Accessibility.Public)
                    Report(context, method, parameter.Locations, Describe(parameter.Type));
            }
        }

        private static void AnalyzeProperty(SymbolAnalysisContext context)
        {
            var property = (IPropertySymbol)context.Symbol;
            if (!IsExposed(property) || !InScope(property) || HasExemption(property))
                return;
            if (property.IsOverride || property.ExplicitInterfaceImplementations.Length > 0)
                return;
            if (IsUntypedValue(property.Type))
                Report(context, property, property.Locations, Describe(property.Type));
        }

        // Public or protected, through every containing type.
        private static bool IsExposed(ISymbol symbol)
        {
            for (ISymbol current = symbol; current != null && !(current is INamespaceSymbol); current = current.ContainingSymbol)
            {
                var exposed = current.DeclaredAccessibility == Accessibility.Public
                    || current.DeclaredAccessibility == Accessibility.Protected
                    || current.DeclaredAccessibility == Accessibility.ProtectedOrInternal;
                if (!exposed) return false;
            }

            return true;
        }

        // The whole surface of a shipped package, and a component's surface wherever it is declared:
        // a declaration class, and the render helpers that put a component on a page.
        private static bool InScope(ISymbol symbol)
        {
            if (ShippedAssemblies.Contains(symbol.ContainingAssembly?.Name ?? ""))
                return true;

            var containingType = symbol.ContainingType;
            if (DerivesFrom(containingType, "Alis.Reactive.JsObject"))
                return true;
            if (symbol is IMethodSymbol method)
            {
                return IsNamed(method.ReturnType, "Alis.Reactive.RenderedComponent<TComponent, TModel>")
                    || (method.IsExtensionMethod
                        && IsNamed(method.Parameters[0].Type, "Alis.Reactive.InputBoundField<TModel, TProp>"));
            }

            return false;
        }

        // An override or an interface implementation whose shape a base class or interface outside the
        // shipped packages fixed (Equals(object), a JSON converter's Read, IHtmlContent.WriteTo).
        private static bool InheritsShapeFromOutside(IMethodSymbol method)
        {
            if (method.IsOverride)
            {
                var root = method;
                while (root.OverriddenMethod != null) root = root.OverriddenMethod;
                return !ShippedAssemblies.Contains(root.ContainingAssembly?.Name ?? "");
            }

            return method.ExplicitInterfaceImplementations
                .Any(implemented => !ShippedAssemblies.Contains(implemented.ContainingAssembly?.Name ?? ""));
        }

        private static bool HasExemption(ISymbol symbol)
        {
            foreach (var attribute in symbol.GetAttributes())
            {
                if (attribute.AttributeClass?.ToDisplayString() == ExemptionAttribute)
                    return true;
            }

            return false;
        }

        private static bool DerivesFrom(INamedTypeSymbol? type, string baseName)
        {
            for (var current = type?.BaseType; current != null; current = current.BaseType)
            {
                if (current.ToDisplayString() == baseName) return true;
            }

            return false;
        }

        private static bool IsNamed(ITypeSymbol type, string definitionName) =>
            type is INamedTypeSymbol named && named.OriginalDefinition.ToDisplayString() == definitionName;

        // object, System.Type, and any array or generic type built over them: the values that let
        // anything through. Expression<Func<T, object?>> selectors are not flagged: the parameter type
        // is Expression<>, whose type argument is a delegate, not a value.
        private static bool IsUntypedValue(ITypeSymbol type)
        {
            if (type.SpecialType == SpecialType.System_Object)
                return true;
            if (type.ToDisplayString() == "System.Type")
                return true;
            if (type is IArrayTypeSymbol array)
                return IsUntypedValue(array.ElementType);
            if (type is INamedTypeSymbol named && named.IsGenericType && !IsDelegate(named))
                return named.TypeArguments.Any(IsUntypedValue);
            return false;
        }

        private static bool IsDelegate(INamedTypeSymbol type) =>
            type.TypeKind == TypeKind.Delegate
            || (type.Name == "Expression" && type.ContainingNamespace?.ToDisplayString() == "System.Linq.Expressions");

        private static bool IsUntypedArgument(ITypeSymbol type) =>
            type.ToDisplayString() == "Alis.Reactive.Arg";

        private static bool IsMemberNamingString(IParameterSymbol parameter) =>
            parameter.Type.SpecialType == SpecialType.System_String
            && MemberNamingParameters.Contains(parameter.Name);

        private static string Describe(ITypeSymbol type) => type.ToDisplayString();

        private static void Report(
            SymbolAnalysisContext context,
            ISymbol member,
            ImmutableArray<Location> locations,
            string offending)
        {
            var location = locations.Length > 0 ? locations[0] : member.Locations[0];
            context.ReportDiagnostic(Diagnostic.Create(Rule, location, member.Name, offending));
        }
    }
}
