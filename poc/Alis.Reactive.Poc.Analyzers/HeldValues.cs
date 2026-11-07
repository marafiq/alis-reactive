using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;

namespace Alis.Reactive.Poc.Analyzers;

internal enum ValueKind
{
    Text,
    Number,
    Boolean,
    Date,
    List,
    Data,
    Untyped,
    Promise,
    Function,
    Nothing,
    Unmirrored,
}

internal readonly record struct VendorValue(ValueKind Kind, string Shown, ContractType? Type, VendorValue[] Items);

internal readonly record struct Misfit(string Clause, string Cure);

internal sealed class HeldValues
{
    private static readonly Regex NumberLiteral = new(@"^-?\d", RegexOptions.CultureInvariant);

    private readonly VendorContract _contract;
    private readonly bool _reading;
    private readonly HashSet<string> _walked = new(StringComparer.Ordinal);

    private HeldValues(VendorContract contract, bool reading)
    {
        _contract = contract;
        _reading = reading;
    }

    public static Misfit? Read(VendorContract contract, ITypeSymbol declared, IReadOnlyList<Held> held, string vendorMember)
        => new HeldValues(contract, reading: true).Reads(declared, held, vendorMember, "");

    public static Misfit? Write(VendorContract contract, ITypeSymbol declared, IReadOnlyList<Held> held, string vendorMember)
        => new HeldValues(contract, reading: false).Writes(declared, held, vendorMember, "");

    public static bool IsPromise(VendorContract contract, Held held) => new HeldValues(contract, reading: true).Of(held).Kind == ValueKind.Promise;

    private Misfit? Reads(ITypeSymbol declared, IReadOnlyList<Held> held, string vendorMember, string path)
    {
        var parts = Present(held).ToList();
        var kinds = parts.Where(part => part.Kind != ValueKind.Untyped).Select(part => part.Kind).Distinct().ToList();
        if (kinds.Count > 1)
        {
            return new Misfit("'" + vendorMember + "' holds " + string.Join(" or ", kinds.Select(Noun)) + ", which no one C# type holds", "a door for it is a gap to name");
        }

        foreach (var part in parts)
        {
            if (Fit(declared, part, vendorMember, path) is { } misfit) return misfit;
        }

        return null;
    }

    private Misfit? Writes(ITypeSymbol declared, IReadOnlyList<Held> held, string vendorMember, string path)
    {
        var parts = Present(held).ToList();
        if (parts.Count == 0) return null;
        Misfit? first = null;
        foreach (var part in parts)
        {
            var misfit = Fit(declared, part, vendorMember, path);
            if (misfit is null) return null;
            first ??= misfit;
        }

        return parts.Count == 1 ? first : new Misfit("'" + vendorMember + "' takes " + string.Join(" or ", parts.Select(Said)) + ", and '" + Shown(declared) + "' is none of them", "declare the type it takes");
    }

    private IEnumerable<VendorValue> Present(IReadOnlyList<Held> held) => held.Where(each => each.Shape != HeldShape.Null).Select(Of);

    private Misfit? Fit(ITypeSymbol declared, VendorValue vendor, string vendorMember, string path)
    {
        var bare = Bare(declared);
        switch (vendor.Kind)
        {
            case ValueKind.Untyped:
                return null;
            case ValueKind.Promise:
                return new Misfit("'" + vendorMember + "' holds a promise", "no door awaits a promise a member holds, a gap to name");
            case ValueKind.Function:
                return new Misfit("'" + vendorMember + "' holds a function", "declare it with Method, MethodAsync or Callback");
            case ValueKind.Nothing:
                return new Misfit("'" + vendorMember + "' returns nothing", "declare it with the door that returns no value");
            case ValueKind.Unmirrored:
                return new Misfit("'" + vendorMember + "' holds " + Said(vendor) + ", which no C# type mirrors", "a door for it is a gap to name");
        }

        var kind = KindOf(bare);
        if (vendor.Kind == ValueKind.List)
        {
            if (kind != ValueKind.List || ItemOf(bare) is not { } item) return Mismatch(path, bare, vendor, vendorMember);
            foreach (var each in vendor.Items)
            {
                if (Fit(item, each, vendorMember, path) is not { } misfit) continue;
                if (each.Kind == ValueKind.Data) return misfit;
                return new Misfit("'" + vendorMember + "' holds a list of " + Noun(each.Kind), "declare it as a list of " + CSharp(each));
            }

            return null;
        }

        if (!Takes(bare, kind, vendor.Kind)) return Mismatch(path, bare, vendor, vendorMember);
        if (kind != ValueKind.Data || vendor.Type is not { } type) return null;
        return Members(bare, type, path);
    }

    private bool Takes(ITypeSymbol declared, ValueKind kind, ValueKind vendor)
    {
        if (kind == vendor) return true;
        if (declared.TypeKind == TypeKind.Enum) return vendor is ValueKind.Text or ValueKind.Number;
        return _reading && kind == ValueKind.Date && vendor == ValueKind.Text;
    }

    private Misfit? Members(ITypeSymbol data, ContractType type, string path)
    {
        if (!_walked.Add(data.ToDisplayString() + "|" + type.Key)) return null;
        foreach (var property in DataMembers(data))
        {
            var name = JsonName(property);
            var at = (path.Length == 0 ? Shown(data) : path) + "." + property.Name;
            if (_contract.Member(type, name) is not { CanRead: true } member)
            {
                return new Misfit("'" + at + "' reads '" + name + "', which '" + type.Shown + "' does not hold", "name it as '" + type.Shown + "' does");
            }

            if (Reads(property.Type, member.Held("value"), type.Shown + "." + name, at) is { } misfit) return misfit;
        }

        return null;
    }

    private static Misfit Mismatch(string path, ITypeSymbol declared, VendorValue vendor, string vendorMember)
    {
        var holds = "'" + vendorMember + "' holds " + Said(vendor);
        var clause = path.Length == 0 ? holds : holds + ", and '" + path + "' is '" + Shown(declared) + "'";
        return new Misfit(clause, "declare " + Cure(vendor));
    }

    private static string Cure(VendorValue vendor) => "it as " + CSharp(vendor);

    private static string CSharp(VendorValue vendor) => vendor.Kind switch
    {
        ValueKind.Text => "string",
        ValueKind.Number => "a number type: int, long, decimal or double",
        ValueKind.Boolean => "bool",
        ValueKind.Date => "DateTime or DateTimeOffset",
        ValueKind.List => "a list of what each item holds",
        _ => "a type whose members mirror '" + vendor.Shown + "'",
    };

    private static string Noun(ValueKind kind) => kind switch
    {
        ValueKind.Text => "text",
        ValueKind.Number => "numbers",
        ValueKind.Boolean => "booleans",
        ValueKind.Date => "dates",
        ValueKind.List => "lists",
        ValueKind.Data => "objects",
        _ => "values",
    };

    private static string Said(VendorValue vendor) => vendor.Kind switch
    {
        ValueKind.Text => "text",
        ValueKind.Number => "a number",
        ValueKind.Boolean => "a boolean",
        ValueKind.Date => "a date",
        ValueKind.List => "a list",
        _ => "'" + vendor.Shown + "'",
    };

    private static string Shown(ITypeSymbol type) => type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

    private VendorValue Of(Held held) => held.Shape switch
    {
        HeldShape.Object => Object(held.Key),
        HeldShape.Items => new VendorValue(ValueKind.List, "a list", null, [Object(held.Key)]),
        HeldShape.Function or HeldShape.PromiseFunction => new VendorValue(ValueKind.Function, "a function", null, []),
        HeldShape.Self => new VendorValue(ValueKind.Untyped, "the object itself", null, []),
        _ => Written(held.Text),
    };

    private VendorValue Object(TypeKey key)
    {
        var name = BaseName(key.Name);
        var declaredByTheLanguage = key.Module.StartsWith("lib.", StringComparison.Ordinal);
        if (declaredByTheLanguage && name is "Promise" or "PromiseLike") return new VendorValue(ValueKind.Promise, "Promise", null, []);
        if (declaredByTheLanguage && name == "Date") return new VendorValue(ValueKind.Date, "Date", null, []);
        if (declaredByTheLanguage && name == "Object") return new VendorValue(ValueKind.Untyped, "Object", null, []);
        var type = _contract.Type(key);
        return new VendorValue(ValueKind.Data, type?.Shown ?? name, type is { IsDuplicate: false } ? type : null, []);
    }

    private static string BaseName(string name)
    {
        var mark = name.IndexOf('#');
        return mark < 0 ? name : name.Substring(0, mark);
    }

    private static VendorValue Written(string text)
    {
        if (text is "string" || text.StartsWith("\"", StringComparison.Ordinal) || text.StartsWith("`", StringComparison.Ordinal)) return new VendorValue(ValueKind.Text, text, null, []);
        if (text is "number" || NumberLiteral.IsMatch(text)) return new VendorValue(ValueKind.Number, text, null, []);
        if (text is "boolean" or "true" or "false") return new VendorValue(ValueKind.Boolean, text, null, []);
        if (text is "any" or "unknown" or "object" or "{}") return new VendorValue(ValueKind.Untyped, text, null, []);
        if (text is "void" or "undefined" or "never") return new VendorValue(ValueKind.Nothing, text, null, []);
        var list = text.StartsWith("readonly ", StringComparison.Ordinal) ? text.Substring("readonly ".Length) : text;
        if (list.EndsWith("[]", StringComparison.Ordinal))
        {
            var item = list.Substring(0, list.Length - 2);
            var grouped = item.StartsWith("(", StringComparison.Ordinal) && item.EndsWith(")", StringComparison.Ordinal);
            var parts = grouped ? item.Substring(1, item.Length - 2).Split([" | "], StringSplitOptions.None) : [item];
            var items = parts.Where(part => part is not ("null" or "undefined")).Select(Written).ToArray();
            return new VendorValue(ValueKind.List, text, null, items);
        }

        return new VendorValue(ValueKind.Unmirrored, text, null, []);
    }

    private static ITypeSymbol Bare(ITypeSymbol type)
        => type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } wrapped
            ? wrapped.TypeArguments[0]
            : type.WithNullableAnnotation(NullableAnnotation.NotAnnotated);

    private static ValueKind KindOf(ITypeSymbol type)
    {
        switch (type.SpecialType)
        {
            case SpecialType.System_String or SpecialType.System_Char:
                return ValueKind.Text;
            case SpecialType.System_Boolean:
                return ValueKind.Boolean;
            case SpecialType.System_Byte or SpecialType.System_SByte or SpecialType.System_Int16 or SpecialType.System_UInt16
                or SpecialType.System_Int32 or SpecialType.System_UInt32 or SpecialType.System_Int64 or SpecialType.System_UInt64
                or SpecialType.System_Decimal or SpecialType.System_Double or SpecialType.System_Single:
                return ValueKind.Number;
            case SpecialType.System_DateTime:
                return ValueKind.Date;
        }

        if (type.ToDisplayString() == "System.DateTimeOffset") return ValueKind.Date;
        if (type.ToDisplayString() == "System.Guid") return ValueKind.Text;
        if (type.TypeKind == TypeKind.Enum) return ValueKind.Unmirrored;
        if (ItemOf(type) is not null) return ValueKind.List;
        var data = type.TypeKind is TypeKind.Class or TypeKind.Struct && type.SpecialType == SpecialType.None && !InSystem(type);
        return data ? ValueKind.Data : ValueKind.Unmirrored;
    }

    private static bool InSystem(ITypeSymbol type)
    {
        var space = type.ContainingNamespace?.ToDisplayString() ?? "";
        return space == "System" || space.StartsWith("System.", StringComparison.Ordinal);
    }

    private static ITypeSymbol? ItemOf(ITypeSymbol type)
    {
        if (type is IArrayTypeSymbol array) return array.ElementType;
        if (type.SpecialType == SpecialType.System_String) return null;
        if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Collections_Generic_IEnumerable_T } sequence) return sequence.TypeArguments[0];
        var keyed = type.AllInterfaces.Any(each => each.OriginalDefinition.ToDisplayString() is "System.Collections.Generic.IReadOnlyDictionary<TKey, TValue>" or "System.Collections.Generic.IDictionary<TKey, TValue>");
        if (keyed) return null;
        return type.AllInterfaces.FirstOrDefault(each => each.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T)?.TypeArguments[0];
    }

    private static IEnumerable<IPropertySymbol> DataMembers(ITypeSymbol data)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        for (var type = data; type is not null && !InSystem(type); type = type.BaseType)
        {
            foreach (var property in type.GetMembers().OfType<IPropertySymbol>())
            {
                var written = property.DeclaredAccessibility == Accessibility.Public && !property.IsStatic && !property.IsIndexer && property.GetMethod is not null;
                if (!written || Ignored(property) || !seen.Add(property.Name)) continue;
                yield return property;
            }
        }
    }

    private static bool Ignored(IPropertySymbol property)
        => property.GetAttributes().Any(attribute => attribute.AttributeClass?.ToDisplayString() == "System.Text.Json.Serialization.JsonIgnoreAttribute");

    private static string JsonName(IPropertySymbol property)
    {
        foreach (var attribute in property.GetAttributes())
        {
            var named = attribute.AttributeClass?.ToDisplayString() == "System.Text.Json.Serialization.JsonPropertyNameAttribute";
            if (named && attribute.ConstructorArguments.Length == 1 && attribute.ConstructorArguments[0].Value is string name) return name;
        }

        return CamelCase.Of(property.Name);
    }
}
