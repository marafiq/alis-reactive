using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Alis.Reactive.Analyzers.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ALIS009PublicDoorIsTyped : DiagnosticAnalyzer
{
    public const string Id = "ALIS009";

    private const string Category = "Alis.Reactive.Analyzers";
    private const string Core = "Alis.Reactive";
    private const string ExemptionName = "Alis.Reactive.Declarations.TypedDslExemptionAttribute";
    private const string TypeName = "System.Type";

    private static readonly ImmutableArray<string> CallerInformation = ImmutableArray.Create(
        "System.Runtime.CompilerServices.CallerMemberNameAttribute",
        "System.Runtime.CompilerServices.CallerFilePathAttribute",
        "System.Runtime.CompilerServices.CallerArgumentExpressionAttribute");

    private static readonly DiagnosticDescriptor NamesAnUntypedType = new(
        Id,
        "A public door names the type of every value",
        "'{0}' is a public door and names '{1}': a door is typed; write the value's own type, a type parameter or a typed source",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor TakesANamingString = new(
        Id,
        "A public door takes no string that names a member",
        "'{0}' takes the string '{1}', named for '{2}': a public door names a member, field, event, method or path by a typed lambda or a declaration, never by a string",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor ExemptionSaysWhy = new(
        Id,
        "An exemption from ALIS009 says why",
        "An exemption from ALIS009 says why; give TypedDslExemption its reason",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly ImmutableArray<DiagnosticDescriptor> Rules
        = ImmutableArray.Create(NamesAnUntypedType, TakesANamingString, ExemptionSaysWhy);

    private static readonly ImmutableHashSet<string> NamingWords = ImmutableHashSet.Create(
        StringComparer.OrdinalIgnoreCase,
        "action",
        "collection",
        "event",
        "field",
        "kind",
        "member",
        "method",
        "op",
        "owner",
        "param",
        "path",
        "peer",
        "property",
        "selector",
        "subscribe",
        "unsubscribe");

    private static readonly SymbolDisplayFormat AsTheDeveloperNamesIt = new(
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypes,
        memberOptions: SymbolDisplayMemberOptions.IncludeContainingType,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

    private enum Reason
    {
        None,
        Given,
        Empty,
        Unread,
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Rules;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(Start);
    }

    private static void Start(CompilationStartAnalysisContext start)
    {
        if (SliceTable.Read(start.Options, start.CancellationToken) is null) return;

        var surface = new Surface(
            start.Compilation.Assembly,
            start.Compilation.GetTypeByMetadataName(TypeName),
            start.Compilation.GetTypeByMetadataName(ExemptionName),
            CallerInformation.Select(start.Compilation.GetTypeByMetadataName).OfType<INamedTypeSymbol>().ToImmutableArray());
        start.RegisterSymbolAction(
            context => Read(context, surface),
            SymbolKind.NamedType,
            SymbolKind.Method,
            SymbolKind.Property,
            SymbolKind.Field,
            SymbolKind.Event);
    }

    private static void Read(SymbolAnalysisContext context, Surface surface)
    {
        var door = context.Symbol;
        if (door is IMethodSymbol { MethodKind: not (MethodKind.Ordinary or MethodKind.Constructor or MethodKind.UserDefinedOperator or MethodKind.Conversion) }) return;
        if (door is IMethodSymbol { PartialDefinitionPart: not null } or IPropertySymbol { PartialDefinitionPart: not null }) return;

        var exemption = surface.ExemptionOn(door);
        var reason = ReasonOf(exemption);
        if (reason == Reason.Empty) context.ReportDiagnostic(Diagnostic.Create(ExemptionSaysWhy, WhereTheReasonIs(exemption!, context.CancellationToken)));
        if (reason is Reason.Given or Reason.Unread || !IsADoor(door)) return;

        var refused = new List<Diagnostic>();
        switch (door)
        {
            case IMethodSymbol method:
                Untyped(refused, surface, door, method.ReturnType, NameOf(door));
                Parameters(refused, surface, door, method.Parameters);
                Constraints(refused, surface, door, method.TypeParameters);
                break;
            case IPropertySymbol property:
                Untyped(refused, surface, door, property.Type, NameOf(door));
                Parameters(refused, surface, door, property.Parameters);
                break;
            case IFieldSymbol field:
                Untyped(refused, surface, door, field.Type, NameOf(door));
                break;
            case IEventSymbol raised:
                Untyped(refused, surface, door, raised.Type, NameOf(door));
                break;
            case INamedTypeSymbol type:
                Declared(refused, surface, type);
                break;
        }

        if (refused.Count == 0 || surface.IsShapedOutside(door)) return;
        foreach (var report in refused) context.ReportDiagnostic(report);
    }

    private static void Declared(List<Diagnostic> refused, Surface surface, INamedTypeSymbol type)
    {
        var name = NameOf(type);
        if (type.DelegateInvokeMethod is { } invoke)
        {
            Untyped(refused, surface, type, invoke.ReturnType, name);
            Parameters(refused, surface, type, invoke.Parameters);
        }

        if (type.BaseType is { } parent) UntypedArgument(refused, surface, type, parent, name);
        foreach (var contract in type.Interfaces) UntypedArgument(refused, surface, type, contract, name);
        Constraints(refused, surface, type, type.TypeParameters);
    }

    private static void Parameters(List<Diagnostic> refused, Surface surface, ISymbol door, ImmutableArray<IParameterSymbol> parameters)
    {
        foreach (var parameter in parameters)
        {
            var name = NameOf(parameter);
            Untyped(refused, surface, door, parameter.Type, name);
            if (IsStrings(parameter.Type) && !surface.IsFilledByTheCompiler(parameter) && NamedFor(parameter.Name) is { } word)
                refused.Add(Diagnostic.Create(TakesANamingString, name, Named(door), parameter.Name, word));
        }
    }

    private static void Constraints(List<Diagnostic> refused, Surface surface, ISymbol door, ImmutableArray<ITypeParameterSymbol> typeParameters)
    {
        foreach (var typeParameter in typeParameters)
        {
            var name = NameOf(typeParameter);
            foreach (var constraint in typeParameter.ConstraintTypes) Untyped(refused, surface, door, constraint, name);
        }
    }

    private static void Untyped(List<Diagnostic> refused, Surface surface, ISymbol door, ITypeSymbol type, Location name)
    {
        if (surface.UntypedIn(type) is { } untyped) refused.Add(Names(door, untyped, name));
    }

    private static void UntypedArgument(List<Diagnostic> refused, Surface surface, ISymbol door, INamedTypeSymbol type, Location name)
    {
        if (surface.UntypedArgumentOf(type) is { } untyped) refused.Add(Names(door, untyped, name));
    }

    private static Diagnostic Names(ISymbol door, ITypeSymbol untyped, Location name)
        => Diagnostic.Create(
            NamesAnUntypedType,
            name,
            Named(door),
            untyped.WithNullableAnnotation(NullableAnnotation.None).ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));

    private static string Named(ISymbol door)
    {
        var name = door.ToDisplayString(AsTheDeveloperNamesIt);
        return door is IPropertySymbol { IsIndexer: true } ? name + "[]" : name;
    }

    private static bool IsADoor(ISymbol symbol)
    {
        for (var held = symbol; held is not null and not INamespaceSymbol; held = held.ContainingSymbol)
        {
            if (held.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Protected or Accessibility.ProtectedOrInternal)) return false;
        }

        return true;
    }

    private static Location NameOf(ISymbol written) => written.Locations.FirstOrDefault() ?? Location.None;

    private static bool IsStrings(ITypeSymbol type)
        => type.SpecialType == SpecialType.System_String || (type is IArrayTypeSymbol array && IsStrings(array.ElementType));

    private static string? NamedFor(string name)
    {
        foreach (var word in Words(name))
        {
            if (NamingWords.TryGetValue(word, out var listed) || NamingWords.TryGetValue(OneOf(word), out listed)) return listed;
        }

        return null;
    }

    private static IEnumerable<string> Words(string name)
    {
        var start = 0;
        for (var at = 1; at < name.Length; at++)
        {
            if (!StartsAWord(name, at)) continue;
            yield return name.Substring(start, at - start);
            start = at;
        }

        yield return name.Substring(start);
    }

    private static bool StartsAWord(string name, int at)
    {
        var before = name[at - 1];
        var here = name[at];
        if (char.IsLetter(before) != char.IsLetter(here)) return true;
        if (!char.IsUpper(here)) return false;
        return char.IsLower(before) || (at + 1 < name.Length && char.IsLower(name[at + 1]));
    }

    private static string OneOf(string plural)
    {
        if (plural.EndsWith("ies", StringComparison.OrdinalIgnoreCase)) return plural.Substring(0, plural.Length - 3) + "y";
        return plural.EndsWith("s", StringComparison.OrdinalIgnoreCase) ? plural.Substring(0, plural.Length - 1) : plural;
    }

    private static Reason ReasonOf(AttributeData? exemption)
    {
        if (exemption is null) return Reason.None;
        if (exemption.AttributeConstructor is null || exemption.ConstructorArguments.Length != 1) return Reason.Unread;

        var written = exemption.ConstructorArguments[0];
        if (written.Kind == TypedConstantKind.Error) return Reason.Unread;
        return written.Value is string reason && !string.IsNullOrWhiteSpace(reason) ? Reason.Given : Reason.Empty;
    }

    private static Location WhereTheReasonIs(AttributeData exemption, CancellationToken cancellationToken)
    {
        var written = exemption.ApplicationSyntaxReference?.GetSyntax(cancellationToken);
        if (written is AttributeSyntax { ArgumentList.Arguments: { Count: > 0 } arguments }) return arguments[0].Expression.GetLocation();
        return written?.GetLocation() ?? Location.None;
    }

    private sealed class Surface
    {
        private readonly IAssemblySymbol _project;
        private readonly INamedTypeSymbol? _type;
        private readonly INamedTypeSymbol? _exemption;
        private readonly ImmutableArray<INamedTypeSymbol> _callerInformation;

        internal Surface(IAssemblySymbol project, INamedTypeSymbol? type, INamedTypeSymbol? exemption, ImmutableArray<INamedTypeSymbol> callerInformation)
        {
            _project = project;
            _type = type;
            _exemption = exemption;
            _callerInformation = callerInformation;
        }

        internal bool IsFilledByTheCompiler(IParameterSymbol parameter)
            => parameter.GetAttributes().Any(attribute => attribute.AttributeClass is { } written && _callerInformation.Contains(written, SymbolEqualityComparer.Default));

        internal AttributeData? ExemptionOn(ISymbol door)
        {
            if (_exemption is null || door is not (IMethodSymbol or IPropertySymbol)) return null;
            return door.GetAttributes().FirstOrDefault(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, _exemption));
        }

        internal ITypeSymbol? UntypedIn(ITypeSymbol type)
        {
            switch (type)
            {
                case IDynamicTypeSymbol:
                    return type;
                case IArrayTypeSymbol array:
                    return UntypedIn(array.ElementType);
                case IPointerTypeSymbol pointer:
                    return UntypedIn(pointer.PointedAtType);
                case IFunctionPointerTypeSymbol function:
                    return UntypedIn(function.Signature.ReturnType) ?? function.Signature.Parameters.Select(parameter => UntypedIn(parameter.Type)).FirstOrDefault(found => found is not null);
                case INamedTypeSymbol named:
                    var isUntyped = named.SpecialType == SpecialType.System_Object || SymbolEqualityComparer.Default.Equals(named, _type);
                    return isUntyped ? named : UntypedArgumentOf(named);
                default:
                    return null;
            }
        }

        internal ITypeSymbol? UntypedArgumentOf(INamedTypeSymbol named)
        {
            for (var holder = named; holder is not null; holder = holder.ContainingType)
            {
                foreach (var argument in holder.TypeArguments)
                {
                    if (UntypedIn(argument) is { } found) return found;
                }
            }

            return null;
        }

        internal bool IsShapedOutside(ISymbol door)
        {
            if (Introduced(door) is { } introduced && IsOutside(introduced)) return true;
            if (door.ContainingType is not { } holder) return false;

            foreach (var contract in holder.AllInterfaces)
            {
                if (!IsOutside(contract)) continue;
                foreach (var member in contract.GetMembers())
                {
                    if (SymbolEqualityComparer.Default.Equals(holder.FindImplementationForInterfaceMember(member), door)) return true;
                }
            }

            return false;
        }

        private static ISymbol? Introduced(ISymbol door)
        {
            switch (door)
            {
                case IMethodSymbol { OverriddenMethod: { } overridden }:
                    return Introduced(overridden) ?? overridden;
                case IPropertySymbol { OverriddenProperty: { } overridden }:
                    return Introduced(overridden) ?? overridden;
                case IEventSymbol { OverriddenEvent: { } overridden }:
                    return Introduced(overridden) ?? overridden;
                default:
                    return null;
            }
        }

        private bool IsOutside(ISymbol declared)
        {
            var assembly = declared.OriginalDefinition.ContainingAssembly;
            if (assembly is null || SymbolEqualityComparer.Default.Equals(assembly, _project)) return false;
            if (string.Equals(assembly.Name, Core, StringComparison.Ordinal)) return false;
            return !assembly.Modules.Any(module => module.ReferencedAssemblies.Any(reference => string.Equals(reference.Name, Core, StringComparison.Ordinal)));
        }
    }
}
