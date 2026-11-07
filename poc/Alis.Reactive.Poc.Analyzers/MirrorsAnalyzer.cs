using System;
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
    private static readonly Rules Live = new(ended: false);

    private static readonly Rules Ended = new(ended: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = Live.All.AddRange(Ended.All);

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
            var uses = new ConcurrentDictionary<INamedTypeSymbol, ConcurrentBag<Use>>(SymbolEqualityComparer.Default);
            start.RegisterSymbolAction(symbol => CheckRoot(symbol, framework, contract), SymbolKind.NamedType);
            start.RegisterOperationAction(operation => Collect(operation, framework, contract, mirrored, uses), OperationKind.Invocation);
            if (contract is null) return;
            start.RegisterCompilationEndAction(end => new ReachWalk(end, framework, contract, uses, Ended).Run());
        });
    }

    internal static void CheckMember(Action<Diagnostic> report, Rules rules, VendorContract contract, Use use, ContractType type, string shown)
    {
        var door = use.Door;
        if (door.Shape == DoorShape.Itself)
        {
            CheckItself(report, rules, use, type, shown);
            return;
        }

        if (use.JavaScriptName is not { } javaScriptName) return;
        if (door.Shape == DoorShape.Event)
        {
            if (!contract.DeclaresEvent(type, javaScriptName)) report(Diagnostic.Create(rules.Undeclared, use.Location, use.Declared, javaScriptName, shown));
            return;
        }

        var found = contract.Member(type, javaScriptName);
        var vendorMember = shown + "." + javaScriptName;
        if (found is null)
        {
            report(Diagnostic.Create(rules.NotPublic, use.Location, use.Declared, javaScriptName, shown));
            return;
        }

        if (KindMismatch(door, found) is { } cure)
        {
            report(Diagnostic.Create(rules.Kind, use.Location, use.Declared, door.Name, vendorMember, Describe(found), cure));
            return;
        }

        if (door.Shape == DoorShape.Call)
        {
            if (found.PromiseFromSome)
            {
                report(Diagnostic.Create(rules.PromiseFromSome, use.Location, use.Declared, door.Name, vendorMember));
                return;
            }

            if (door.Awaits != found.ReturnsPromise)
            {
                var says = found.ReturnsPromise ? "returns a promise" : "returns no promise";
                report(Diagnostic.Create(rules.Promise, use.Location, use.Declared, door.Name, vendorMember, says, door.Other));
                return;
            }
        }

        if (door.Reach == Reach.Callback)
        {
            CheckAnswer(report, rules, use, found, vendorMember);
            return;
        }

        CheckNull(report, rules, use, found, vendorMember);
    }

    internal static void CheckPosition(Action<Diagnostic> report, Rules rules, Use use, string declaration, Parameters? parameters, string handedBy)
    {
        if (use.Position is not { } position) return;
        if (parameters is null)
        {
            report(Diagnostic.Create(rules.NotHanded, use.Location, use.Declared, use.Door.Name, position, declaration));
            return;
        }

        if (parameters.At(position) is not { } held)
        {
            report(Diagnostic.Create(rules.NoArgument, use.Location, use.Declared, use.Door.Name, position, handedBy));
            return;
        }

        if (use.Door.Reads)
        {
            if (held.Any(each => each.Shape == HeldShape.Null) && use.Value is { Nullable: false } value)
            {
                report(Diagnostic.Create(rules.ArgumentNull, use.Location, use.Declared, value.Shown, position, handedBy));
            }

            return;
        }

        foreach (var each in held)
        {
            if (each.Shape == HeldShape.PromiseFunction)
            {
                report(Diagnostic.Create(rules.ArgumentPromise, use.Location, use.Declared, position, handedBy));
                return;
            }

            if (each.Shape is HeldShape.Null or HeldShape.Function) continue;
            report(Diagnostic.Create(rules.NotAFunction, use.Location, use.Declared, position, handedBy, Showing(each)));
            return;
        }
    }

    internal static string Showing(Held held) => held.Shape switch
    {
        HeldShape.Function or HeldShape.PromiseFunction => "a function",
        HeldShape.Null => "null or undefined",
        HeldShape.Items => "a list of '" + held.Key.Name + "'",
        HeldShape.Self => "the object itself",
        HeldShape.Object => "'" + held.Key.Name + "'",
        _ => "'" + held.Text + "'",
    };

    private static void CheckRoot(SymbolAnalysisContext context, Framework framework, VendorContract? contract)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        if (!framework.IsRoot(type)) return;
        var attribute = framework.MirrorsOf(type);
        if (attribute is null)
        {
            context.ReportDiagnostic(Diagnostic.Create(Live.Unnamed, type.Locations[0], type.Name));
            return;
        }

        var (module, name) = Framework.Arguments(attribute);
        var problem = contract is null
            ? "and no vendor contract reached the build: import poc/VendorContract.targets"
            : contract.Type(module, name) is not { } found
                ? "which the vendor contract does not list"
                : found.IsDuplicate ? "which the vendor contract lists more than once" : null;
        if (problem is null) return;
        var location = attribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation() ?? type.Locations[0];
        context.ReportDiagnostic(Diagnostic.Create(Live.Unlisted, location, type.Name, name, module, problem));
    }

    private static void Collect(OperationAnalysisContext context, Framework framework, VendorContract? contract, ConcurrentDictionary<INamedTypeSymbol, Mirrored?> mirrored, ConcurrentDictionary<INamedTypeSymbol, ConcurrentBag<Use>> uses)
    {
        var invocation = (IInvocationOperation)context.Operation;
        var door = Door.Of(invocation.TargetMethod, framework);
        if (door is null) return;
        var member = context.ContainingSymbol is IMethodSymbol { AssociatedSymbol: { } associated } ? associated : context.ContainingSymbol;
        if (member.ContainingType is not { } holder || !framework.IsJsObject(holder)) return;
        var use = Use.Of(invocation, door, member, holder);
        uses.GetOrAdd(holder, _ => []).Add(use);
        if (contract is null || !framework.IsRoot(holder)) return;
        var mirror = mirrored.GetOrAdd(holder, root => Mirrored.Of(root, framework, contract));
        if (mirror is null) return;
        if (door.Shape == DoorShape.Position)
        {
            CheckPosition(context.ReportDiagnostic, Live, use, holder.Name, null, "");
            return;
        }

        CheckMember(context.ReportDiagnostic, Live, contract, use, mirror.Type, mirror.Name);
    }

    private static void CheckItself(Action<Diagnostic> report, Rules rules, Use use, ContractType type, string shown)
    {
        if (!type.IsCallable)
        {
            report(Diagnostic.Create(rules.Kind, use.Location, use.Declared, use.Door.Name, shown, "not callable", "the members it has"));
            return;
        }

        if (type.ReturnsPromise)
        {
            report(Diagnostic.Create(rules.Promise, use.Location, use.Declared, use.Door.Name, shown, "returns a promise", "a door that awaits a call of the object itself, a gap to name"));
        }
    }

    private static void CheckAnswer(Action<Diagnostic> report, Rules rules, Use use, ContractMember found, string vendorMember)
    {
        if (use.Answer is { Nullable: true } answer && !found.ReturnsNull)
        {
            report(Diagnostic.Create(rules.AnswersNull, use.Location, use.Declared, answer.Shown, vendorMember));
        }
    }

    private static void CheckNull(Action<Diagnostic> report, Rules rules, Use use, ContractMember found, string vendorMember)
    {
        if (!use.Door.ChecksNull || use.Value is not { } value) return;

        if (use.Door.Shape == DoorShape.Call)
        {
            if (found.ReturnsNull && !value.Nullable) report(Diagnostic.Create(rules.ReturnsNull, use.Location, use.Declared, value.Shown, vendorMember));
            return;
        }

        if (use.Door.Reads && found.ReadsNull && !value.Nullable)
        {
            report(Diagnostic.Create(rules.ReadsNull, use.Location, use.Declared, value.Shown, vendorMember));
            return;
        }

        if (use.Door.Writes && value.Nullable && !found.WritesNull)
        {
            report(Diagnostic.Create(rules.WritesNull, use.Location, use.Declared, value.Shown, vendorMember));
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

    internal sealed class Mirrored
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
            return contract.Type(module, name) is { IsDuplicate: false } type ? new Mirrored(type.Shown, type) : null;
        }
    }
}
