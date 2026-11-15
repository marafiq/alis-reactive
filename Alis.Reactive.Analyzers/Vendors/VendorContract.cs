using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Alis.Reactive.Analyzers.Vendors;

internal sealed class VendorContract
{
    private readonly ImmutableDictionary<(string Module, string Name), int> _types;

    private readonly ImmutableDictionary<(string Module, string Name), (Table Table, string Id)> _roots;

    private VendorContract(ImmutableDictionary<(string Module, string Name), int> types, ImmutableDictionary<(string Module, string Name), (Table Table, string Id)> roots)
    {
        _types = types;
        _roots = roots;
    }

    internal bool TryMember(string module, string type, string name, CancellationToken cancellationToken, out Member? member)
    {
        member = null;
        if (Count(module, type) != 1) return false;
        var root = _roots[(module, type)];
        member = root.Table.Find(root.Id, name, new HashSet<string>(StringComparer.Ordinal), cancellationToken);
        return true;
    }

    internal bool DeclaresEvent(string module, string type, string name, CancellationToken cancellationToken)
    {
        if (Count(module, type) != 1) return false;
        var root = _roots[(module, type)];
        return root.Table.DeclaresEvent(root.Id, name, cancellationToken);
    }

    internal sealed class Member
    {
        internal Member(string kind, bool callable, bool hasSignatures, bool returnsPromise, bool optional, bool readsNull, bool writesNull, bool resultReadsNull, bool? callbackTakesNull, (string Module, string Location)? documentedNull, ImmutableArray<(string Kind, string Value)> events, bool returnsNonFunction, string? eventAnswer)
        {
            Kind = kind;
            Callable = callable;
            HasSignatures = hasSignatures;
            ReturnsPromise = returnsPromise;
            Optional = optional;
            ReadsNull = readsNull;
            WritesNull = writesNull;
            ResultReadsNull = resultReadsNull;
            CallbackTakesNull = callbackTakesNull;
            DocumentedNull = documentedNull;
            Events = events;
            ReturnsNonFunction = returnsNonFunction;
            EventAnswer = eventAnswer;
        }

        internal ImmutableArray<(string Kind, string Value)> Events { get; }
        internal bool ReturnsNonFunction { get; }
        internal string? EventAnswer { get; }
        internal string Kind { get; }
        internal bool Callable { get; }
        internal bool HasSignatures { get; }
        internal bool ReturnsPromise { get; }
        internal bool Optional { get; }
        internal bool ReadsNull { get; }
        internal bool WritesNull { get; }
        internal bool ResultReadsNull { get; }
        internal bool? CallbackTakesNull { get; }
        internal (string Module, string Location)? DocumentedNull { get; }
    }

    private sealed class Table
    {
        private readonly ImmutableDictionary<(string Type, string Name), Member> _members;
        private readonly ImmutableDictionary<string, ImmutableArray<string>> _bases;
        private readonly ImmutableDictionary<string, ImmutableArray<(string Name, Member Member)>> _byType;

        internal Table(ImmutableDictionary<(string Type, string Name), Member> members, ImmutableDictionary<string, ImmutableArray<string>> bases)
        {
            _members = members;
            _bases = bases;
            _byType = members.GroupBy(pair => pair.Key.Type).ToImmutableDictionary(group => group.Key, group => group.Select(pair => (pair.Key.Name, pair.Value)).ToImmutableArray());
        }

        internal bool DeclaresEvent(string type, string name, CancellationToken cancellationToken)
        {
            if (Find(type, name, new HashSet<string>(StringComparer.Ordinal), cancellationToken) is { Kind: "property" or "readonly", Callable: true }) return true;
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var member in Members(type, new HashSet<string>(StringComparer.Ordinal), cancellationToken))
            {
                if (!names.Add(member.Name) || member.Member.Kind != "method") continue;
                foreach (var selector in member.Member.Events)
                {
                    if (selector.Kind == "name" && selector.Value == name) return true;
                    if (selector.Kind == "keys" && Find(selector.Value, name, new HashSet<string>(StringComparer.Ordinal), cancellationToken) is not null) return true;
                }
            }

            return false;
        }

        private IEnumerable<(string Name, Member Member)> Members(string type, HashSet<string> visited, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!visited.Add(type)) yield break;
            if (_byType.TryGetValue(type, out var members))
                foreach (var member in members) yield return member;
            if (!_bases.TryGetValue(type, out var bases)) yield break;
            foreach (var parent in bases)
                foreach (var member in Members(parent, visited, cancellationToken)) yield return member;
        }

        internal Member? Find(string type, string name, HashSet<string> visited, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!visited.Add(type)) return null;
            if (_members.TryGetValue((type, name), out var member)) return member;
            if (!_bases.TryGetValue(type, out var bases)) return null;
            foreach (var parent in bases)
            {
                if (Find(parent, name, visited, cancellationToken) is { } inherited) return inherited;
            }

            return null;
        }
    }

    internal int Count(string module, string name) => _types.TryGetValue((module, name), out var count) ? count : 0;

    internal static bool IsTable(string path)
    {
        var normal = path.Replace('\\', '/');
        return string.Equals(normal.Substring(normal.LastIndexOf('/') + 1), "vendor-contract.tsv", StringComparison.Ordinal);
    }

    internal static VendorContract? Read(AnalyzerOptions options, CancellationToken cancellationToken)
    {
        var types = ImmutableDictionary.CreateBuilder<(string Module, string Name), int>();
        var roots = ImmutableDictionary.CreateBuilder<(string Module, string Name), (Table Table, string Id)>();
        var read = false;
        foreach (var file in options.AdditionalFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!IsTable(file.Path)) continue;
            var text = file.GetText(cancellationToken);
            if (text is null || text.Lines[0].ToString() != "format\t1") continue;
            read = true;
            var declarations = new List<(string Module, string Name, string Id)>();
            var members = new List<string[]>();
            var bases = new Dictionary<string, SortedDictionary<int, string>>(StringComparer.Ordinal);
            var signatures = new Dictionary<string, List<(string Id, string PromiseMode, bool ReadsNull, bool TakesUndefined)>>(StringComparer.Ordinal);
            var events = new Dictionary<string, List<(string Kind, string Value)>>(StringComparer.Ordinal);
            var results = new Dictionary<string, List<(string Kind, string Reference, string Text)>>(StringComparer.Ordinal);
            var returnTypes = new Dictionary<string, string>(StringComparer.Ordinal);
            var resultNulls = new Dictionary<string, bool>(StringComparer.Ordinal);
            var defaultNulls = new Dictionary<string, (string Module, string Location)>(StringComparer.Ordinal);
            foreach (var line in text.Lines)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var columns = line.ToString().Split('\t');
                switch (columns[0])
                {
                    case "type" when columns.Length == 6 && columns[4] is "class" or "interface" or "alias" or "function" or "enum":
                        var module = Unescape(columns[2]);
                        var name = Unescape(columns[3]);
                        types.TryGetValue((module, name), out var count);
                        types[(module, name)] = count + 1;
                        declarations.Add((module, name, Unescape(columns[1])));
                        break;
                    case "member" when columns.Length == 9 && columns[4] is "method" or "property" or "readonly" or "writeonly":
                        members.Add(columns);
                        break;
                    case "event" when columns.Length == 6 && columns[3] is "name" or "keys":
                        var eventMember = Unescape(columns[1]);
                        if (!events.TryGetValue(eventMember, out var selectors)) events[eventMember] = selectors = [];
                        selectors.Add((columns[3], Unescape(columns[4])));
                        break;
                    case "base" when columns.Length == 4 && int.TryParse(columns[2], NumberStyles.None, CultureInfo.InvariantCulture, out var ordinal):
                        var owner = Unescape(columns[1]);
                        if (!bases.TryGetValue(owner, out var parents)) bases[owner] = parents = new SortedDictionary<int, string>();
                        parents[ordinal] = Unescape(columns[3]);
                        break;
                    case "signature" when columns.Length == 8 && columns[2] == "member" && columns[5] is "none" or "promise" or "mixed":
                        var id = Unescape(columns[3]);
                        if (!signatures.TryGetValue(id, out var overloads)) signatures[id] = overloads = [];
                        overloads.Add((Unescape(columns[1]), columns[5], columns[6] == "true", columns[7] == "true"));
                        break;
                    case "return-type" when columns.Length == 3:
                        returnTypes[Unescape(columns[1])] = Unescape(columns[2]);
                        break;
                    case "holds" when columns.Length == 9 && columns[2] == "signature" && columns[4] == "returns":
                        var signature = Unescape(columns[3]);
                        if (!results.TryGetValue(signature, out var held)) results[signature] = held = [];
                        held.Add((columns[6], Unescape(columns[7]), Unescape(columns[8])));
                        resultNulls.TryGetValue(signature, out var takesNull);
                        resultNulls[signature] = takesNull || columns[6] == "null" || columns[6] == "other" && columns[8] is "any" or "unknown";
                        break;
                    case "default-null" when columns.Length == 7 && columns[5] == "true":
                        defaultNulls[Unescape(columns[1])] = (Unescape(columns[2]), Unescape(columns[3]) + ":" + columns[4]);
                        break;
                }
            }

            var publicMembers = ImmutableDictionary.CreateBuilder<(string Type, string Name), Member>();
            foreach (var member in members)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var id = Unescape(member[1]);
                signatures.TryGetValue(id, out var overloads);
                var hasSignatures = overloads is { Count: > 0 };
                var returnsPromise = overloads?.Any(signature => signature.PromiseMode is "promise" or "mixed") == true;
                var resultReadsNull = overloads?.Any(signature => signature.ReadsNull) == true;
                bool? callbackTakesNull = !hasSignatures ? null
                    : overloads!.Any(signature => signature.PromiseMode == "promise" || resultNulls.TryGetValue(signature.Id, out var takesNull) && !takesNull) ? false
                    : overloads.All(signature => signature.PromiseMode == "none" && resultNulls.ContainsKey(signature.Id)) ? true : null;
                var returnsNonFunction = overloads?.Any(signature => results.TryGetValue(signature.Id, out var held) && held.Any(value => value.Kind is not ("function" or "null" or "undefined"))) == true;
                string? eventAnswer = null;
                foreach (var signature in overloads ?? [])
                {
                    if (signature.TakesUndefined && signature.PromiseMode != "promise" || !returnTypes.TryGetValue(signature.Id, out var answer)) continue;
                    eventAnswer = answer;
                    break;
                }

                events.TryGetValue(id, out var memberEvents);
                var documentedNull = defaultNulls.TryGetValue(id, out var tag) ? tag : ((string Module, string Location)?)null;
                publicMembers[(Unescape(member[2]), Unescape(member[3]))] = new Member(member[4], member[8] == "true", hasSignatures, returnsPromise,
                    member[5] == "true", member[6] == "true", member[7] == "true", resultReadsNull, callbackTakesNull, documentedNull, memberEvents?.ToImmutableArray() ?? [], returnsNonFunction, eventAnswer);
            }

            var table = new Table(publicMembers.ToImmutable(), bases.ToImmutableDictionary(pair => pair.Key, pair => pair.Value.Values.ToImmutableArray()));
            foreach (var declaration in declarations) roots[(declaration.Module, declaration.Name)] = (table, declaration.Id);
        }

        return read ? new VendorContract(types.ToImmutable(), roots.ToImmutable()) : null;
    }

    private static string Unescape(string text)
    {
        if (text.IndexOf('\\') < 0) return text;
        var result = new StringBuilder(text.Length);
        for (var at = 0; at < text.Length; at++)
        {
            var character = text[at];
            if (character == '\\' && at + 1 < text.Length)
            {
                character = text[++at] switch
                {
                    't' => '\t',
                    'r' => '\r',
                    'n' => '\n',
                    var escaped => escaped,
                };
            }

            result.Append(character);
        }

        return result.ToString();
    }
}
