using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Alis.Reactive.Poc.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MirrorsAnalyzer : DiagnosticAnalyzer
{
    public const string Category = "Alis.Reactive.Contract";
    public const string UnnamedId = "ALIS014";
    public const string UnlistedId = "ALIS015";
    public const string NotPublicId = "ALIS016";
    public const string PromiseId = "ALIS017";
    public const string KindId = "ALIS018";
    public const string NullId = "ALIS019";

    private static readonly DiagnosticDescriptor Unnamed = Rule(
        UnnamedId,
        "A component or plugin names the TypeScript type it mirrors",
        "'{0}' names no TypeScript type it mirrors: mark it [Mirrors(module, type)] with the type its vendor's declarations give it");

    private static readonly DiagnosticDescriptor Unlisted = Rule(
        UnlistedId,
        "The type a declaration mirrors is in the vendor contract",
        "'{0}' mirrors '{1}' of '{2}', {3}");

    private static readonly DiagnosticDescriptor NotPublic = Rule(
        NotPublicId,
        "A declared member is public in the vendor's typed API",
        "'{0}' declares '{1}', which '{2}' does not make public: declare only what the vendor's typed API makes public");

    private static readonly DiagnosticDescriptor Promise = Rule(
        PromiseId,
        "A method that returns a promise is declared with MethodAsync",
        "'{0}' is declared with {1}, and '{2}' {3}: declare it with {4}");

    private static readonly DiagnosticDescriptor PromiseFromSome = Rule(
        PromiseId,
        "A method that returns a promise is declared with MethodAsync",
        "'{0}' is declared with {1}, and '{2}' returns a promise from some of its signatures and not from others, which no declaration mirrors");

    private static readonly DiagnosticDescriptor Kind = Rule(
        KindId,
        "A member is declared with the door its kind takes",
        "'{0}' is declared with {1}, and '{2}' is {3}: declare it with {4}");

    private static readonly DiagnosticDescriptor ReadsNull = Rule(
        NullId,
        "A declaration's nullability fits its vendor's typed API",
        "'{0}' reads '{1}', and '{2}' can read null or undefined: declare '{1}?'");

    private static readonly DiagnosticDescriptor ReturnsNull = Rule(
        NullId,
        "A declaration's nullability fits its vendor's typed API",
        "'{0}' returns '{1}', and '{2}' can return null or undefined: declare '{1}?'");

    private static readonly DiagnosticDescriptor WritesNull = Rule(
        NullId,
        "A declaration's nullability fits its vendor's typed API",
        "'{0}' writes '{1}?', and '{2}' takes no null: declare '{1}'");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Unnamed, Unlisted, NotPublic, Promise, PromiseFromSome, Kind, ReadsNull, ReturnsNull, WritesNull);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            var framework = Framework.Of(start.Compilation);
            if (framework is null) return;
            var contract = VendorContract.Read(start.Options.AdditionalFiles, start.CancellationToken);
            var mirrored = new ConcurrentDictionary<INamedTypeSymbol, Mirrored?>(SymbolEqualityComparer.Default);
            start.RegisterSymbolAction(symbol => CheckRoot(symbol, framework, contract), SymbolKind.NamedType);
            start.RegisterOperationAction(operation => CheckDoor(operation, framework, contract, mirrored), OperationKind.Invocation);
        });
    }

    private static DiagnosticDescriptor Rule(string id, string title, string message)
        => new(id, title, message, Category, DiagnosticSeverity.Error, isEnabledByDefault: true);

    private static void CheckRoot(SymbolAnalysisContext context, Framework framework, VendorContract? contract)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        if (!framework.IsRoot(type)) return;
        var attribute = framework.MirrorsOf(type);
        if (attribute is null)
        {
            context.ReportDiagnostic(Diagnostic.Create(Unnamed, type.Locations[0], type.Name));
            return;
        }

        var (module, name) = Framework.Arguments(attribute);
        var problem = contract is null
            ? "and no vendor contract reached the build: import poc/VendorContract.targets"
            : contract.Type(module, name) is not { } found
                ? "which the vendor contract does not list"
                : found.IsDuplicate ? "which its module declares more than once" : null;
        if (problem is null) return;
        var location = attribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation() ?? type.Locations[0];
        context.ReportDiagnostic(Diagnostic.Create(Unlisted, location, type.Name, name, module, problem));
    }

    private static void CheckDoor(OperationAnalysisContext context, Framework framework, VendorContract? contract, ConcurrentDictionary<INamedTypeSymbol, Mirrored?> mirrored)
    {
        if (contract is null) return;
        var invocation = (IInvocationOperation)context.Operation;
        var door = Door.Of(invocation.TargetMethod, framework);
        if (door is null) return;
        var member = context.ContainingSymbol is IMethodSymbol { AssociatedSymbol: { } associated } ? associated : context.ContainingSymbol;
        if (member.ContainingType is not { } holder || !framework.IsRoot(holder)) return;
        var mirror = mirrored.GetOrAdd(holder, root => Mirrored.Of(root, framework, contract));
        if (mirror is null) return;
        var declared = holder.Name + "." + member.Name;
        var location = member.Locations[0];

        if (door.Shape == DoorShape.Itself)
        {
            CheckItself(context, door, mirror, declared, location);
            return;
        }

        var javaScriptName = JavaScriptName(invocation, member.Name);
        if (javaScriptName is null) return;
        var found = contract.Member(mirror.Type, javaScriptName);
        var vendorMember = mirror.Name + "." + javaScriptName;
        if (found is null)
        {
            context.ReportDiagnostic(Diagnostic.Create(NotPublic, location, declared, javaScriptName, mirror.Name));
            return;
        }

        if (KindMismatch(door, found) is { } cure)
        {
            context.ReportDiagnostic(Diagnostic.Create(Kind, location, declared, door.Name, vendorMember, Describe(found), cure));
            return;
        }

        if (door.Shape == DoorShape.Call)
        {
            if (found.PromiseFromSome)
            {
                context.ReportDiagnostic(Diagnostic.Create(PromiseFromSome, location, declared, door.Name, vendorMember));
                return;
            }

            if (door.Awaits != found.ReturnsPromise)
            {
                var says = found.ReturnsPromise ? "returns a promise" : "returns no promise";
                context.ReportDiagnostic(Diagnostic.Create(Promise, location, declared, door.Name, vendorMember, says, door.Other));
                return;
            }
        }

        CheckNull(context, invocation, door, found, declared, vendorMember, location);
    }

    private static void CheckItself(OperationAnalysisContext context, Door door, Mirrored mirror, string declared, Location location)
    {
        if (!mirror.Type.IsCallable)
        {
            context.ReportDiagnostic(Diagnostic.Create(Kind, location, declared, door.Name, mirror.Name, "not callable", "the members it has"));
            return;
        }

        if (mirror.Type.ReturnsPromise)
        {
            context.ReportDiagnostic(Diagnostic.Create(Promise, location, declared, door.Name, mirror.Name, "returns a promise", "a door that awaits a call of the object itself, a gap to name"));
        }
    }

    private static void CheckNull(OperationAnalysisContext context, IInvocationOperation invocation, Door door, ContractMember found, string declared, string vendorMember, Location location)
    {
        if (!door.ChecksNull || invocation.TargetMethod.TypeArguments.Length != 1) return;
        var value = invocation.TargetMethod.TypeArguments[0];
        if (value is ITypeParameterSymbol) return;
        var nullable = value.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T
            || invocation.TargetMethod.TypeArgumentNullableAnnotations[0] == NullableAnnotation.Annotated;
        var bare = value is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } wrapped
            ? wrapped.TypeArguments[0]
            : value.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
        var shown = bare.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

        if (door.Shape == DoorShape.Call)
        {
            if (found.ReturnsNull && !nullable) context.ReportDiagnostic(Diagnostic.Create(ReturnsNull, location, declared, shown, vendorMember));
            return;
        }

        if (door.Reads && found.ReadsNull && !nullable)
        {
            context.ReportDiagnostic(Diagnostic.Create(ReadsNull, location, declared, shown, vendorMember));
            return;
        }

        if (door.Writes && nullable && !found.WritesNull)
        {
            context.ReportDiagnostic(Diagnostic.Create(WritesNull, location, declared, shown, vendorMember));
        }
    }

    private static string? KindMismatch(Door door, ContractMember found)
    {
        if (door.Shape == DoorShape.Call) return found.CanCall ? null : ValueDoor(found);
        if (found.Kind == MemberKind.Method) return "Method or MethodAsync";
        if (door.Reads && !found.CanRead) return "WriteOnly";
        if (door.Writes && !found.CanWrite) return "ReadOnly";
        return null;
    }

    private static string ValueDoor(ContractMember found) => found.Kind switch
    {
        MemberKind.ReadOnly => "ReadOnly",
        MemberKind.WriteOnly => "WriteOnly",
        _ => "Property",
    };

    private static string Describe(ContractMember found) => found.Kind switch
    {
        MemberKind.Property => found.IsCallable ? "a property" : "a value, not a method",
        MemberKind.ReadOnly => found.IsCallable ? "read-only" : "a read-only value, not a method",
        MemberKind.WriteOnly => "write-only",
        _ => "a method",
    };

    private static string? JavaScriptName(IInvocationOperation invocation, string memberName)
    {
        var argument = invocation.Arguments.FirstOrDefault(each => each.Parameter?.Name == "name");
        if (argument is null || argument.ArgumentKind == ArgumentKind.DefaultValue) return CamelCase.Of(memberName);
        return argument.Value.ConstantValue is { HasValue: true, Value: string name } ? name : null;
    }

    private sealed class Mirrored
    {
        private Mirrored(string name, ContractType type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; }

        public ContractType Type { get; }

        public static Mirrored? Of(INamedTypeSymbol root, Framework framework, VendorContract contract)
        {
            var attribute = framework.MirrorsOf(root);
            if (attribute is null) return null;
            var (module, name) = Framework.Arguments(attribute);
            return contract.Type(module, name) is { IsDuplicate: false } type ? new Mirrored(name, type) : null;
        }
    }
}
