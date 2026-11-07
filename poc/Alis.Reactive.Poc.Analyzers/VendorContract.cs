using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Alis.Reactive.Poc.Analyzers;

internal sealed class VendorContract
{
    public const string FileName = "vendor-contract.tsv";

    private const string EventSlot = "event:";

    private readonly Dictionary<TypeKey, ContractType> _types;

    private VendorContract(Dictionary<TypeKey, ContractType> types)
    {
        _types = types;
    }

    public static VendorContract? Read(ImmutableArray<AdditionalText> files, CancellationToken cancellationToken)
    {
        var types = new Dictionary<TypeKey, ContractType>();
        var read = false;
        foreach (var file in files)
        {
            if (!file.Path.EndsWith(FileName, StringComparison.Ordinal)) continue;
            var text = file.GetText(cancellationToken);
            if (text is null) continue;
            read = true;
            foreach (var type in Parsed(text.ToString()))
            {
                types[type.Key] = types.ContainsKey(type.Key) ? ContractType.Duplicate(type.Key) : type;
            }
        }

        return read ? new VendorContract(types) : null;
    }

    public ContractType? Type(string module, string name) => Type(new TypeKey(module, name));

    public ContractType? Type(TypeKey key) => _types.TryGetValue(key, out var type) ? type : null;

    public ContractMember? Member(ContractType type, string name)
    {
        foreach (var owner in WithBases(type))
        {
            if (owner.Members.TryGetValue(name, out var member)) return member;
        }

        return null;
    }

    public bool DeclaresEvent(ContractType type, string name)
    {
        foreach (var owner in WithBases(type))
        {
            foreach (var member in owner.Members.Values)
            {
                if (IsEventProperty(member, name)) return true;
                if (member.EventNames.Contains(name)) return true;
                foreach (var map in member.EventMaps)
                {
                    if (_types.TryGetValue(map, out var keys) && Member(keys, name) is not null) return true;
                }
            }
        }

        return false;
    }

    public Parameters? Payload(ContractType type, string name)
    {
        var parameters = new Parameters();
        var found = false;
        foreach (var owner in WithBases(type))
        {
            foreach (var member in owner.Members.Values)
            {
                if (IsEventProperty(member, name))
                {
                    found = true;
                    parameters.Add(member.Holds, "");
                }

                if (member.EventNames.Contains(name))
                {
                    found = true;
                    parameters.Add(member.Holds, EventSlot + name + ":");
                }

                foreach (var map in member.EventMaps)
                {
                    if (!_types.TryGetValue(map, out var keys) || Member(keys, name) is not { } keyed) continue;
                    found = true;
                    parameters.AddFirst(keyed.Held("value"));
                }
            }
        }

        return found ? parameters : null;
    }

    public static Parameters CallbackParameters(ContractMember member)
    {
        var parameters = new Parameters();
        parameters.Add(member.Holds, "");
        return parameters;
    }

    private static bool IsEventProperty(ContractMember member, string name)
        => member.Name == name && member.Kind != MemberKind.Method && member.IsCallable;

    private IEnumerable<ContractType> WithBases(ContractType type)
    {
        var visited = new HashSet<TypeKey>();
        var pending = new Stack<ContractType>();
        pending.Push(type);
        while (pending.Count > 0)
        {
            var current = pending.Pop();
            if (!visited.Add(current.Key)) continue;
            yield return current;
            for (var position = current.Bases.Count - 1; position >= 0; position--)
            {
                if (_types.TryGetValue(current.Bases[position], out var basis)) pending.Push(basis);
            }
        }
    }

    private static Dictionary<TypeKey, ContractType>.ValueCollection Parsed(string text)
    {
        var types = new Dictionary<TypeKey, ContractType>();
        var members = new List<string[]>();
        var holds = new List<string[]>();
        foreach (var line in text.Split('\n'))
        {
            var columns = line.Split('\t');
            if (columns.Length == 7 && columns[0] == "type") types[new TypeKey(columns[1], columns[2])] = new ContractType(new TypeKey(columns[1], columns[2]), columns[3], Flags(columns[4]), Bases(columns[5]), columns[6]);
            else if (columns.Length == 7 && columns[0] == "member") members.Add(columns);
            else if (columns.Length == 8 && columns[0] == "holds") holds.Add(columns);
        }

        foreach (var columns in members)
        {
            if (!types.TryGetValue(new TypeKey(columns[1], columns[2]), out var owner)) continue;
            owner.Members[columns[3]] = new ContractMember(columns[3], Kind(columns[4]), Flags(columns[5]), Events(columns[6]));
        }

        foreach (var columns in holds)
        {
            if (!types.TryGetValue(new TypeKey(columns[1], columns[2]), out var owner) || !owner.Members.TryGetValue(columns[3], out var member)) continue;
            member.Hold(columns[4], new Held(Shape(columns[5]), new TypeKey(columns[6], columns[7]), columns[7]));
        }

        return types.Values;
    }

    private static (ImmutableHashSet<string> Names, List<TypeKey> Maps) Events(string column)
    {
        var names = ImmutableHashSet.CreateBuilder<string>(StringComparer.Ordinal);
        var maps = new List<TypeKey>();
        if (column.Length == 0) return (names.ToImmutable(), maps);
        foreach (var entry in column.Split(','))
        {
            if (entry.StartsWith("name:", StringComparison.Ordinal)) names.Add(entry.Substring("name:".Length));
            else if (entry.StartsWith("keys:", StringComparison.Ordinal)) maps.Add(Key(entry.Substring("keys:".Length)));
        }

        return (names.ToImmutable(), maps);
    }

    private static TypeKey Key(string reference)
    {
        var separator = reference.LastIndexOf(':');
        return new TypeKey(reference.Substring(0, separator), reference.Substring(separator + 1));
    }

    private static ImmutableHashSet<string> Flags(string column)
        => column.Length == 0 ? ImmutableHashSet<string>.Empty : ImmutableHashSet.Create(StringComparer.Ordinal, column.Split(','));

    private static List<TypeKey> Bases(string column)
    {
        var bases = new List<TypeKey>();
        if (column.Length == 0) return bases;
        foreach (var basis in column.Split(','))
        {
            bases.Add(Key(basis));
        }

        return bases;
    }

    private static MemberKind Kind(string column) => column switch
    {
        "property" => MemberKind.Property,
        "readonly" => MemberKind.ReadOnly,
        "writeonly" => MemberKind.WriteOnly,
        _ => MemberKind.Method,
    };

    private static HeldShape Shape(string column) => column switch
    {
        "object" => HeldShape.Object,
        "items" => HeldShape.Items,
        "self" => HeldShape.Self,
        "function" => HeldShape.Function,
        "promisefunction" => HeldShape.PromiseFunction,
        "null" => HeldShape.Null,
        _ => HeldShape.Other,
    };
}

internal readonly record struct TypeKey(string Module, string Name);

internal enum HeldShape
{
    Object,
    Items,
    Self,
    Function,
    PromiseFunction,
    Null,
    Other,
}

internal readonly record struct Held(HeldShape Shape, TypeKey Key, string Text);

internal sealed class Parameters
{
    private const string Fixed = "parameter:";

    private const string Rest = "rest:";

    private readonly SortedDictionary<int, List<Held>> _fixed = [];

    private readonly SortedDictionary<int, List<Held>> _rest = [];

    public void AddFirst(IReadOnlyList<Held> held) => Merge(_fixed, 0, held);

    public void Add(IReadOnlyDictionary<string, List<Held>> holds, string prefix)
    {
        foreach (var slot in holds)
        {
            if (!slot.Key.StartsWith(prefix, StringComparison.Ordinal)) continue;
            var rest = slot.Key.Substring(prefix.Length);
            if (rest.StartsWith(Fixed, StringComparison.Ordinal) && int.TryParse(rest.Substring(Fixed.Length), out var position)) Merge(_fixed, position, slot.Value);
            else if (rest.StartsWith(Rest, StringComparison.Ordinal) && int.TryParse(rest.Substring(Rest.Length), out var from)) Merge(_rest, from, slot.Value);
        }
    }

    public IReadOnlyList<Held>? At(int position)
    {
        if (_fixed.TryGetValue(position, out var held)) return held;
        List<Held>? rest = null;
        foreach (var start in _rest)
        {
            if (start.Key <= position) rest = start.Value;
        }

        return rest;
    }

    private static void Merge(SortedDictionary<int, List<Held>> slots, int position, IReadOnlyList<Held> held)
    {
        if (!slots.TryGetValue(position, out var list)) slots[position] = list = [];
        foreach (var each in held)
        {
            if (!list.Contains(each)) list.Add(each);
        }
    }
}

internal sealed class ContractType
{
    public ContractType(TypeKey key, string kind, ImmutableHashSet<string> flags, List<TypeKey> bases, string text)
    {
        Key = key;
        IsDuplicate = kind == "duplicate";
        IsCallable = flags.Contains("callable");
        ReturnsPromise = flags.Contains("promise");
        Bases = bases;
        Shown = text.Length == 0 ? key.Name : text;
    }

    public TypeKey Key { get; }

    public bool IsDuplicate { get; }

    public bool IsCallable { get; }

    public bool ReturnsPromise { get; }

    public List<TypeKey> Bases { get; }

    public string Shown { get; }

    public Dictionary<string, ContractMember> Members { get; } = new(StringComparer.Ordinal);

    public static ContractType Duplicate(TypeKey key) => new(key, "duplicate", ImmutableHashSet<string>.Empty, [], "");
}

internal enum MemberKind
{
    Property,
    ReadOnly,
    WriteOnly,
    Method,
}

internal sealed class ContractMember
{
    private static readonly List<Held> None = [];

    private readonly Dictionary<string, List<Held>> _holds = new(StringComparer.Ordinal);

    public ContractMember(string name, MemberKind kind, ImmutableHashSet<string> flags, (ImmutableHashSet<string> Names, List<TypeKey> Maps) events)
    {
        EventNames = events.Names;
        EventMaps = events.Maps;
        Name = name;
        Kind = kind;
        IsCallable = flags.Contains("callable");
        ReturnsPromise = flags.Contains("promise");
        PromiseFromSome = flags.Contains("mixed");
        ReadsNull = flags.Contains("readsnull");
        WritesNull = flags.Contains("writesnull");
        ReturnsNull = flags.Contains("returnsnull");
        IsOptional = flags.Contains("optional");
    }

    public string Name { get; }

    public MemberKind Kind { get; }

    public bool IsOptional { get; }

    public bool IsCallable { get; }

    public bool ReturnsPromise { get; }

    public bool PromiseFromSome { get; }

    public bool ReadsNull { get; }

    public bool WritesNull { get; }

    public bool ReturnsNull { get; }

    public ImmutableHashSet<string> EventNames { get; }

    public List<TypeKey> EventMaps { get; }

    public IReadOnlyDictionary<string, List<Held>> Holds => _holds;

    public bool CanRead => Kind is MemberKind.Property or MemberKind.ReadOnly;

    public bool CanWrite => Kind is MemberKind.Property or MemberKind.WriteOnly;

    public bool CanCall => Kind == MemberKind.Method || (CanRead && IsCallable);

    public IReadOnlyList<Held> Held(string slot) => _holds.TryGetValue(slot, out var held) ? held : None;

    public void Hold(string slot, Held held)
    {
        if (!_holds.TryGetValue(slot, out var list)) _holds[slot] = list = [];
        if (!list.Contains(held)) list.Add(held);
    }
}
