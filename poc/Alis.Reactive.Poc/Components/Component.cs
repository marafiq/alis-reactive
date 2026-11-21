using System;
using System.Collections.Generic;
using Alis.Reactive.Plan;
using Alis.Reactive.Writing;

namespace Alis.Reactive.Components;

public abstract class Component : EventedObject
{
    public abstract ComponentVendor Vendor { get; }

    protected internal sealed override EventListening Listening => Vendor.Listening;
}

public sealed class ComponentVendor
{
    private const string Element = "element";

    private ComponentVendor(string name, Vendor grammar, IReadOnlyList<PathStep>? elementPath)
    {
        Name = name;
        Grammar = grammar;
        ElementPath = elementPath;
        Listening = EventListening.Of(grammar.Listen);
    }

    public static ComponentVendor Native { get; } = new(
        "native",
        new Vendor(
            new FnExpression([Element], new RefExpression(Element)),
            MethodListening.EventTarget),
        []);

    public string Name { get; }

    internal Vendor Grammar { get; }

    internal IReadOnlyList<PathStep>? ElementPath { get; }

    internal EventListening Listening { get; }

    public static ComponentVendor Define(string name, string objectPath, string subscribe, string unsubscribe, string? elementPath = null)
        => new(VendorName(name), new Vendor(AtPath(objectPath), new MethodListening(JavaScriptName(subscribe), JavaScriptName(unsubscribe))), ElementPathOf(elementPath));

    public static ComponentVendor DefineCallbackProperties(string name, string objectPath, string? elementPath = null)
        => new(VendorName(name), new Vendor(AtPath(objectPath), new AssignmentListening()), ElementPathOf(elementPath));

    public static ComponentVendor DefineReturnedUnsubscriber(string name, string objectPath, string subscribe, string? elementPath = null)
        => new(VendorName(name), new Vendor(AtPath(objectPath), new ReturnedListening(JavaScriptName(subscribe))), ElementPathOf(elementPath));

    public static ComponentVendor Define(string name, VendorLookup lookup, string subscribe, string unsubscribe, string? elementPath = null)
        => new(VendorName(name), lookup.Vendor(new MethodListening(JavaScriptName(subscribe), JavaScriptName(unsubscribe))), ElementPathOf(elementPath));

    public static ComponentVendor DefineCallbackProperties(string name, VendorLookup lookup, string? elementPath = null)
        => new(VendorName(name), lookup.Vendor(new AssignmentListening()), ElementPathOf(elementPath));

    public static ComponentVendor DefineReturnedUnsubscriber(string name, VendorLookup lookup, string subscribe, string? elementPath = null)
        => new(VendorName(name), lookup.Vendor(new ReturnedListening(JavaScriptName(subscribe))), ElementPathOf(elementPath));

    public override string ToString() => Name;

    private static FnExpression AtPath(string objectPath)
    {
        var steps = PathSteps(objectPath);
        var body = steps.Count == 0
            ? (Expression)new RefExpression(Element)
            : new GetExpression(new RefExpression(Element), steps, ReadPresence.Required);
        return new FnExpression([Element], body);
    }

    private static List<PathStep>? ElementPathOf(string? elementPath) => elementPath is null ? null : PathSteps(elementPath);

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
        if (!JavaScriptNames.IsRegistryName(name)) throw new ArgumentException($"Vendor name '{name}' must start with a letter and contain only ASCII letters, digits, underscore, or hyphen.", nameof(name));
        return name;
    }

    private static string JavaScriptName(string name) => JavaScriptNames.Checked(name);
}

public sealed class VendorLookup
{
    private const string Element = "element";

    private readonly string _plugin;

    private VendorLookup(string plugin)
    {
        _plugin = plugin;
    }

    public static VendorLookup ByPlugin(string name)
    {
        if (!JavaScriptNames.IsRegistryName(name))
        {
            throw new ArgumentException(
                $"Plugin name '{name}' must start with a letter and contain only ASCII letters, digits, underscore, or hyphen. A name with a dot is the framework's own.",
                nameof(name));
        }

        return new VendorLookup(name);
    }

    internal Vendor Vendor(Listening listen)
    {
        var found = new CallExpression(new ObjectExpression(PlanBuild.PluginKey(_plugin)), [], ReadPresence.Required, [new RefExpression(Element)]);
        var lookup = new PluginLocator(_plugin, string.Empty, MethodListening.EventTarget);
        return new Vendor(new FnExpression([Element], found), listen, [lookup]);
    }
}
