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
            named.Append('<').Append(string.Join(", ", Enumerable.Range(0, arity).Select(_ => AsCSharpWritesIt(arguments.Dequeue())))).Append('>');
            at = digits - 1;
        }

        return named.ToString();
    }

    internal static string AsCSharpWritesIt(Type type)
    {
        if (Nullable.GetUnderlyingType(type) is { } underlying) return AsCSharpWritesIt(underlying) + "?";
        var name = NameOf(type);
        if (type.IsArray) return name;
        var arity = name.IndexOf(ArityMark);
        if (arity < 0) return name;
        return name.Substring(0, arity) + "<" + string.Join(", ", type.GetGenericArguments().Select(AsCSharpWritesIt)) + ">";
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
}
