using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Alis.Reactive.TypeKnowledge;

internal static class ClassNames
{
    private const char ArityMark = '`';
    private const char ArgumentsOpen = '[';
    private const char RankOpen = '[';

    private static readonly Dictionary<Type, string> Keywords = new()
    {
        [typeof(bool)] = "bool",
        [typeof(byte)] = "byte",
        [typeof(sbyte)] = "sbyte",
        [typeof(char)] = "char",
        [typeof(decimal)] = "decimal",
        [typeof(double)] = "double",
        [typeof(float)] = "float",
        [typeof(int)] = "int",
        [typeof(uint)] = "uint",
        [typeof(nint)] = "nint",
        [typeof(nuint)] = "nuint",
        [typeof(long)] = "long",
        [typeof(ulong)] = "ulong",
        [typeof(short)] = "short",
        [typeof(ushort)] = "ushort",
        [typeof(object)] = "object",
        [typeof(string)] = "string",
    };

    internal static string ModelKey(Type model)
        => model.FullName ?? throw new InvalidOperationException("A plan's model is a type with a full name.");

    internal static string InNamespace(Type declaring)
    {
        var full = declaring.FullName ?? throw new InvalidOperationException("A member's declaring class is a type with a full name.");
        var opens = full.IndexOf(ArgumentsOpen);
        var written = opens < 0 ? full : full.Substring(0, opens);
        var arguments = new Queue<Type>(declaring.GetGenericArguments());
        var named = new StringBuilder(written.Length);
        for (var at = 0; at < written.Length; at++)
        {
            if (written[at] != ArityMark)
            {
                named.Append(written[at]);
                continue;
            }

            var digits = at + 1;
            while (digits < written.Length && char.IsDigit(written[digits])) digits++;
            var arity = int.Parse(written.Substring(at + 1, digits - at - 1), CultureInfo.InvariantCulture);
            named.Append('<').Append(string.Join(", ", Enumerable.Range(0, arity).Select(_ => ByTypeNames(arguments.Dequeue())))).Append('>');
            at = digits - 1;
        }

        return named.ToString();
    }

    internal static string AsCSharpWritesIt(Type type)
    {
        if (Keywords.TryGetValue(type, out var keyword)) return keyword;
        return type.IsArray ? ElementThenRanks(type, AsCSharpWritesIt) : Composed(type, AsCSharpWritesIt);
    }

    private static string ElementThenRanks(Type array, Func<Type, string> named)
    {
        var ranks = new StringBuilder();
        var element = array;
        while (element.IsArray)
        {
            var level = LevelOf(element);
            ranks.Append(level.Rank);
            element = level.Element;
        }

        return named(element) + ranks;
    }

    private static string ByTypeNames(Type type) => Composed(type, ByTypeNames);

    private static string Composed(Type type, Func<Type, string> named)
    {
        if (Nullable.GetUnderlyingType(type) is { } underlying) return named(underlying) + "?";
        var name = NameOf(type);
        if (type.IsArray) return name;
        var arity = name.IndexOf(ArityMark);
        if (arity < 0) return name;
        return name.Substring(0, arity) + "<" + string.Join(", ", type.GetGenericArguments().Select(named)) + ">";
    }

    internal static string PartialViewOf(Type type)
    {
        if (type.GetGenericArguments().Length != 0)
            throw new InvalidOperationException($"The member's type {AsCSharpWritesIt(type)} is generic and names no partial view: pass the view name to Html.PartialFor.");
        return NameOf(type);
    }

    internal static bool IsInterface(Type type) => type.IsInterface;

    [SuppressMessage("ApiDesign", "RS0030:Do not use banned APIs", Justification = "A type's name is its identity (D18.1), and D14.3 names a generic declaring class's type arguments by it.")]
    private static string NameOf(Type type) => type.Name;

    [SuppressMessage("ApiDesign", "RS0030:Do not use banned APIs", Justification = "An array's own name ends in its rank's brackets, and its element type is the owner's read for a message to name an array as C# writes it (D18.1; the owner, 2026-11-16: \"approved for change string[]\").")]
    private static (string Rank, Type Element) LevelOf(Type array)
    {
        var name = array.Name;
        var element = array.GetElementType() ?? throw new InvalidOperationException("An array is a type with an element type.");
        return (name.Substring(name.LastIndexOf(RankOpen)), element);
    }
}
