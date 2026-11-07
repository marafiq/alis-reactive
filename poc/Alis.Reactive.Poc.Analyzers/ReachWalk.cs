using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Alis.Reactive.Poc.Analyzers;

internal sealed class ReachWalk
{
    private readonly CompilationAnalysisContext _context;
    private readonly Framework _framework;
    private readonly VendorContract _contract;
    private readonly ConcurrentDictionary<INamedTypeSymbol, ConcurrentBag<Use>> _uses;
    private readonly Rules _rules;
    private readonly Queue<Visit> _pending = new();
    private readonly HashSet<string> _visited = new(StringComparer.Ordinal);
    private readonly HashSet<string> _reported = new(StringComparer.Ordinal);

    public ReachWalk(CompilationAnalysisContext context, Framework framework, VendorContract contract, ConcurrentDictionary<INamedTypeSymbol, ConcurrentBag<Use>> uses, Rules rules)
    {
        _context = context;
        _framework = framework;
        _contract = contract;
        _uses = uses;
        _rules = rules;
    }

    public void Run()
    {
        var roots = _uses.Keys.Where(_framework.IsRoot).OrderBy(Named, StringComparer.Ordinal);
        foreach (var root in roots)
        {
            if (MirrorsAnalyzer.Mirrored.Of(root, _framework, _contract) is not { } mirror) continue;
            Enqueue(new Visit(root, mirror.Type, null, "", live: true));
        }

        while (_pending.Count > 0)
        {
            Walk(_pending.Dequeue());
        }
    }

    private static string Named(INamedTypeSymbol type) => type.ToDisplayString();

    private void Enqueue(Visit visit)
    {
        if (_visited.Add(Named(visit.Declaration) + "|" + visit.Type?.Key + "|" + visit.HandedBy)) _pending.Enqueue(visit);
    }

    private void Report(Diagnostic diagnostic)
    {
        var where = diagnostic.Location.GetLineSpan();
        if (_reported.Add(diagnostic.Id + "|" + where.Path + "|" + where.StartLinePosition + "|" + diagnostic.GetMessage(System.Globalization.CultureInfo.InvariantCulture))) _context.ReportDiagnostic(diagnostic);
    }

    private List<Use> UsesOf(INamedTypeSymbol declaration)
    {
        var found = new List<Use>();
        for (var type = declaration; type is not null && _framework.IsJsObject(type); type = type.BaseType)
        {
            if (_uses.TryGetValue(type, out var uses)) found.AddRange(uses);
        }

        return [.. found.OrderBy(use => use.Location.SourceTree?.FilePath, StringComparer.Ordinal).ThenBy(use => use.Location.SourceSpan.Start)];
    }

    private void Walk(Visit visit)
    {
        foreach (var use in UsesOf(visit.Declaration))
        {
            if (use.Door.Shape == DoorShape.Position)
            {
                if (!visit.Live) MirrorsAnalyzer.CheckPosition(Report, _rules, use, visit.Declaration.Name, visit.Parameters, visit.HandedBy);
                continue;
            }

            if (visit.Type is not { } type) continue;
            if (!visit.Live) MirrorsAnalyzer.CheckMember(Report, _rules, _contract, use, type, type.Shown);
            if (use.Door.Reach != Reach.None) Follow(use, type);
        }
    }

    private void Follow(Use use, ContractType type)
    {
        if (use.JavaScriptName is not { } name || use.Reached is not { } reached) return;
        var where = "'" + type.Shown + "." + name + "'";
        switch (use.Door.Reach)
        {
            case Reach.One or Reach.Many:
                if (_contract.Member(type, name) is not { } member || !Reaches(use, member)) return;
                Bind(use, reached, member.Held(member.Kind != MemberKind.Method && use.Door.Shape == DoorShape.Call ? "returns" : "value"), use.Door.Reach == Reach.Many, where, type, null, "");
                return;
            case Reach.Payload:
                if (_contract.Payload(type, name) is not { } payload) return;
                Hand(use, reached, payload, "the listener of '" + type.Shown + "' event '" + name + "'", type);
                return;
            case Reach.Callback:
                if (_contract.Member(type, name) is not { CanWrite: true } callback) return;
                Hand(use, reached, VendorContract.CallbackParameters(callback), where, type);
                return;
        }
    }

    private static bool Reaches(Use use, ContractMember member)
        => use.Door.Shape == DoorShape.Call ? member.CanCall && !member.ReturnsPromise : member.CanRead;

    private void Hand(Use use, INamedTypeSymbol reached, Parameters parameters, string handedBy, ContractType at)
    {
        var readsMembers = UsesOf(reached).Any(each => each.Door.Shape != DoorShape.Position);
        if (!readsMembers)
        {
            Enqueue(new Visit(reached, null, parameters, handedBy, live: false));
            return;
        }

        Bind(use, reached, parameters.At(0) ?? [], many: false, "the first parameter of " + handedBy, at, parameters, handedBy);
    }

    private void Bind(Use use, INamedTypeSymbol reached, IReadOnlyList<Held> held, bool many, string where, ContractType self, Parameters? parameters, string handedBy)
    {
        var present = held.Where(each => each.Shape != HeldShape.Null).ToList();
        if (present.Count == 0) ReportReach(use, reached, many, where, "null or undefined");
        foreach (var each in present)
        {
            if (Resolve(each, many, self) is { } type)
            {
                Enqueue(new Visit(reached, type, parameters, handedBy, live: false));
                continue;
            }

            if (!many && each.Shape == HeldShape.Items)
            {
                Report(Diagnostic.Create(_rules.ReachesAList, use.Location, use.Declared, reached.Name, where, Shown(each.Key), use.Door.Name + "s"));
                continue;
            }

            ReportReach(use, reached, many, where, many && each.Shape is HeldShape.Object or HeldShape.Self ? MirrorsAnalyzer.Showing(each) + ", no list" : MirrorsAnalyzer.Showing(each));
        }
    }

    private ContractType? Resolve(Held held, bool many, ContractType self)
    {
        var expected = many ? HeldShape.Items : HeldShape.Object;
        if (!many && held.Shape == HeldShape.Self) return self;
        if (held.Shape != expected) return null;
        return _contract.Type(held.Key) is { IsDuplicate: false } type ? type : null;
    }

    private string Shown(TypeKey key) => _contract.Type(key)?.Shown ?? key.Name;

    private void ReportReach(Use use, INamedTypeSymbol reached, bool many, string where, string typedAs)
    {
        var rule = many ? _rules.ReachesNoList : _rules.ReachesNoObject;
        var single = use.Door.Name.EndsWith("s", StringComparison.Ordinal) ? use.Door.Name.Substring(0, use.Door.Name.Length - 1) : use.Door.Name;
        Report(many
            ? Diagnostic.Create(rule, use.Location, use.Declared, reached.Name, where, typedAs, single)
            : Diagnostic.Create(rule, use.Location, use.Declared, reached.Name, where, typedAs));
    }

    private sealed class Visit
    {
        public Visit(INamedTypeSymbol declaration, ContractType? type, Parameters? parameters, string handedBy, bool live)
        {
            Declaration = declaration;
            Type = type;
            Parameters = parameters;
            HandedBy = handedBy;
            Live = live;
        }

        public INamedTypeSymbol Declaration { get; }

        public ContractType? Type { get; }

        public Parameters? Parameters { get; }

        public string HandedBy { get; }

        public bool Live { get; }
    }
}
