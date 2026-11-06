using System;
using System.Collections.Generic;
using Alis.Reactive.Plan;

namespace Alis.Reactive.Components;

public abstract class Component : JsObject
{
    public abstract ComponentVendor Vendor { get; }
}

public sealed class ComponentVendor
{
    private const string Element = "element";

    private ComponentVendor(string name, Vendor grammar)
    {
        Name = name;
        Grammar = grammar;
    }

    public static ComponentVendor Native { get; } = new(
        "native",
        new Vendor(
            new FnExpression([Element], new RefExpression(Element)),
            new MethodListening("addEventListener", "removeEventListener")));

    public string Name { get; }

    internal Vendor Grammar { get; }

    public static ComponentVendor Define(string name, string objectPath, string subscribe, string unsubscribe)
        => new(VendorName(name), new Vendor(AtPath(objectPath), new MethodListening(JavaScriptName(subscribe), JavaScriptName(unsubscribe))));

    public override string ToString() => Name;

    private static FnExpression AtPath(string objectPath)
    {
        var steps = PathSteps(objectPath);
        var body = steps.Count == 0
            ? (Expression)new RefExpression(Element)
            : new GetExpression(new RefExpression(Element), steps, ReadPresence.Required);
        return new FnExpression([Element], body);
    }

    private static List<PathStep> PathSteps(string objectPath)
    {
        var steps = new List<PathStep>();
        if (objectPath.Length == 0) return steps;
        foreach (var segment in objectPath.Split('.'))
            steps.Add(IsPosition(segment) ? PathStep.Index(int.Parse(segment, System.Globalization.CultureInfo.InvariantCulture)) : PathStep.Member(JavaScriptName(segment)));
        return steps;
    }

    private static bool IsPosition(string segment)
    {
        if (segment.Length == 0) return false;
        foreach (var character in segment)
        {
            if (character < '0' || character > '9') return false;
        }

        return true;
    }

    private static string VendorName(string name)
    {
        var valid = name.Length > 0 && IsLetter(name[0]);
        foreach (var character in name)
            valid = valid && (IsLetter(character) || (character >= '0' && character <= '9') || character == '_' || character == '-');
        if (!valid) throw new ArgumentException($"Vendor name '{name}' must start with a letter and contain only ASCII letters, digits, underscore, or hyphen.", nameof(name));
        return name;
    }

    private static string JavaScriptName(string name)
    {
        var valid = name.Length > 0 && (IsLetter(name[0]) || name[0] == '_' || name[0] == '$');
        foreach (var character in name)
            valid = valid && (IsLetter(character) || (character >= '0' && character <= '9') || character == '_' || character == '$');
        if (!valid) throw new ArgumentException($"'{name}' is not a JavaScript name: a letter, '_' or '$', then letters, digits, '_' or '$'.", nameof(name));
        return name;
    }

    private static bool IsLetter(char character) => (character >= 'a' && character <= 'z') || (character >= 'A' && character <= 'Z');
}
